using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Web.Clients;

namespace Web.Workers;

public class ProcessingJobWorker : BackgroundService
{
    private const int BatchSize = 10;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _pollingInterval;
    private readonly SemaphoreSlim _iterationLock = new(1, 1);

    public ProcessingJobWorker(
        IServiceScopeFactory scopeFactory,
        TimeSpan? pollingInterval = null)
    {
        _scopeFactory = scopeFactory;
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(10);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingJobsAsync(stoppingToken);
            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    public async Task<int> ProcessPendingJobsAsync(CancellationToken cancellationToken = default)
    {
        if (!await _iterationLock.WaitAsync(0, cancellationToken))
        {
            return 0;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider
                .GetRequiredService<IRepository<ProcessingJob>>();
            var documentRepository = scope.ServiceProvider
                .GetRequiredService<IRepository<Document>>();
            var processingApiClient = scope.ServiceProvider
                .GetRequiredService<IProcessingApiClient>();
            var jobs = await repository.GetAllAsync<ProcessingJob>(
                x => x,
                x => x.Status == ProcessingJobStatus.Pending,
                x => x.OrderBy(job => job.CreatedAt),
                take: BatchSize);

            if (jobs.Count == 0)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            foreach (var job in jobs)
            {
                job.Status = ProcessingJobStatus.Processing;
                job.StartedAt = now;
                job.ModifiedAt = now;
                await repository.UpdateAsync(job);
            }

            await repository.SaveChangesAsync();

            foreach (var job in jobs)
            {
                try
                {
                    var document = await documentRepository.GetAsync<Document>(
                        x => x, x => x.Id == job.DocumentId);
                    if (document == null)
                    {
                        throw new FileNotFoundException("Processing document was not found.");
                    }

                    await processingApiClient.SendProcessingJobAsync(
                        job, document, cancellationToken);
                }
                catch (Exception exception) when (
                    exception is HttpRequestException ||
                    exception is InvalidOperationException ||
                    exception is TaskCanceledException ||
                    exception is FileNotFoundException ||
                    exception is ArgumentException)
                {
                    job.Status = ProcessingJobStatus.Failed;
                    job.ErrorMessage = "Processing API communication failed.";
                    job.FinishedAt = DateTime.UtcNow;
                    job.ModifiedAt = job.FinishedAt.Value;
                    await repository.UpdateAsync(job);
                    await repository.SaveChangesAsync();
                }
            }

            return jobs.Count;
        }
        finally
        {
            _iterationLock.Release();
        }
    }

    public override void Dispose()
    {
        _iterationLock.Dispose();
        base.Dispose();
    }
}
