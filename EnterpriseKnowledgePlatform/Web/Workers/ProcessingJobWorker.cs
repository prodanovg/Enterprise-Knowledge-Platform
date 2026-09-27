using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Service.Interface;
using Domain.Dto.Notifications;
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
            var notificationService = scope.ServiceProvider
                .GetRequiredService<INotificationService>();
            var emailService = scope.ServiceProvider
                .GetRequiredService<IEmailService>();
            var userRepository = scope.ServiceProvider
                .GetRequiredService<IRepository<User>>();
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

                    var result = await processingApiClient.SendProcessingJobAsync(
                        job, document, cancellationToken);
                    if (await IsAlreadyCompletedAsync(repository, job.Id))
                    {
                        job.Status = ProcessingJobStatus.Completed;
                        job.ErrorMessage = null;
                        await NotifyUserAsync(job, document, userRepository,
                            notificationService, emailService, cancellationToken);
                        continue;
                    }

                    job.FinishedAt = DateTime.UtcNow;
                    job.ModifiedAt = job.FinishedAt.Value;
                    job.Status = result.Success
                        ? ProcessingJobStatus.Completed
                        : ProcessingJobStatus.Failed;
                    job.ErrorMessage = result.Success
                        ? null
                        : "FastAPI processing failed.";
                    await repository.UpdateAsync(job);
                    await repository.SaveChangesAsync();
                    await NotifyUserAsync(job, document, userRepository,
                        notificationService, emailService, cancellationToken);
                }
                catch (Exception exception) when (
                    exception is HttpRequestException ||
                    exception is InvalidOperationException ||
                    exception is TaskCanceledException ||
                    exception is FileNotFoundException ||
                    exception is ArgumentException ||
                    exception is IOException)
                {
                    if (await IsAlreadyCompletedAsync(repository, job.Id))
                    {
                        job.Status = ProcessingJobStatus.Completed;
                        job.ErrorMessage = null;
                        var completedDocument = await documentRepository.GetAsync<Document>(
                            x => x, x => x.Id == job.DocumentId);
                        if (completedDocument != null)
                        {
                            await NotifyUserAsync(job, completedDocument, userRepository,
                                notificationService, emailService, cancellationToken);
                        }

                        continue;
                    }

                    job.Status = ProcessingJobStatus.Failed;
                    job.ErrorMessage = "Processing API communication failed.";
                    job.FinishedAt = DateTime.UtcNow;
                    job.ModifiedAt = job.FinishedAt.Value;
                    await repository.UpdateAsync(job);
                    await repository.SaveChangesAsync();
                    var document = await documentRepository.GetAsync<Document>(
                        x => x, x => x.Id == job.DocumentId);
                    if (document != null)
                    {
                        await NotifyUserAsync(job, document, userRepository,
                            notificationService, emailService, cancellationToken);
                    }
                }
            }

            return jobs.Count;
        }
        finally
        {
            _iterationLock.Release();
        }
    }

    private static async Task NotifyUserAsync(
        ProcessingJob job,
        Document document,
        IRepository<User> userRepository,
        INotificationService notificationService,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetAsync<User>(
                x => x, x => x.Id == document.OwnerId);
            if (user == null)
            {
                return;
            }

            var completed = job.Status == ProcessingJobStatus.Completed;
            var title = completed ? "Document processing completed" : "Document processing failed";
            var message = completed
                ? "Your document processing has completed successfully."
                : "Your document processing has failed.";
            await notificationService.CreateAsync(
                new CreateNotificationDto { Title = title, Message = message, SentAt = DateTime.UtcNow },
                user.Id);
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await emailService.SendAsync(user.Email, title, message, cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
        }
    }

    private static async Task<bool> IsAlreadyCompletedAsync(
        IRepository<ProcessingJob> repository,
        Guid processingJobId)
    {
        var currentJob = await repository.GetAsync<ProcessingJob>(
            x => x,
            x => x.Id == processingJobId,
            asNoTracking: true);
        return currentJob?.Status == ProcessingJobStatus.Completed;
    }

    public override void Dispose()
    {
        _iterationLock.Dispose();
        base.Dispose();
    }
}
