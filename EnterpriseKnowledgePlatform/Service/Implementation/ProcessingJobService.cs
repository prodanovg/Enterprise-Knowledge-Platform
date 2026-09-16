using Domain.Dto;
using Domain.Dto.ProcessingJobs;
using Domain.Enums;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class ProcessingJobService : IProcessingJobService
{
    private readonly IRepository<ProcessingJob> _processingJobRepository;
    private readonly IRepository<Document> _documentRepository;

    public ProcessingJobService(
        IRepository<ProcessingJob> processingJobRepository,
        IRepository<Document> documentRepository)
    {
        _processingJobRepository = processingJobRepository;
        _documentRepository = documentRepository;
    }

    public async Task<ProcessingJob> CreateAsync(
        CreateProcessingJobDto dto, string userId)
    {
        var document = await _documentRepository.GetAsync<Document>(
            selector: x => x,
            predicate: x => x.Id == dto.DocumentId && x.OwnerId == userId);

        if (document == null)
        {
            throw new KeyNotFoundException(
                $"Document with ID '{dto.DocumentId}' was not found.");
        }

        var now = DateTime.UtcNow;
        var processingJob = new ProcessingJob
        {
            DocumentId = dto.DocumentId,
            Status = ProcessingJobStatus.Pending,
            CreatedAt = now,
            CreatedBy = userId,
            ModifiedAt = now,
            ModifiedBy = userId
        };

        await _processingJobRepository.InsertAsync(processingJob);
        await _processingJobRepository.SaveChangesAsync();

        return processingJob;
    }

    public async Task<List<ProcessingJob>> StartProcessingAsync(
        IEnumerable<Guid> documentIds, string userId)
    {
        var ids = documentIds.Distinct().ToList();
        var documents = new List<Document>();

        foreach (var documentId in ids)
        {
            var document = await _documentRepository.GetAsync<Document>(
                selector: x => x,
                predicate: x => x.Id == documentId && x.OwnerId == userId);

            if (document == null)
            {
                throw new KeyNotFoundException(
                    $"Document with ID '{documentId}' was not found.");
            }

            documents.Add(document);
        }

        var jobs = new List<ProcessingJob>();
        var now = DateTime.UtcNow;

        foreach (var document in documents)
        {
            var activeJob = await _processingJobRepository
                .GetAsync<ProcessingJob>(
                    selector: x => x,
                    predicate: x => x.DocumentId == document.Id &&
                        (x.Status == ProcessingJobStatus.Pending ||
                         x.Status == ProcessingJobStatus.Processing));

            if (activeJob != null)
            {
                continue;
            }

            var job = new ProcessingJob
            {
                DocumentId = document.Id,
                Status = ProcessingJobStatus.Pending,
                CreatedAt = now,
                CreatedBy = userId,
                ModifiedAt = now,
                ModifiedBy = userId
            };

            await _processingJobRepository.InsertAsync(job);
            jobs.Add(job);
        }

        if (jobs.Count > 0)
        {
            await _processingJobRepository.SaveChangesAsync();
        }

        return jobs;
    }

    public Task<ProcessingJob?> GetByIdAsync(Guid id, string userId)
    {
        return _processingJobRepository.GetAsync<ProcessingJob>(
            selector: x => x,
            predicate: x => x.Id == id && x.Document.OwnerId == userId,
            asNoTracking: true);
    }

    public async Task<ProcessingJob> RetryAsync(Guid id, string userId)
    {
        var job = await _processingJobRepository.GetAsync<ProcessingJob>(
            x => x, x => x.Id == id && x.Document.OwnerId == userId);
        if (job == null)
        {
            throw new KeyNotFoundException($"Processing job with ID '{id}' was not found.");
        }
        if (job.Status != ProcessingJobStatus.Failed)
        {
            throw new InvalidOperationException("Only failed processing jobs can be retried.");
        }

        job.Status = ProcessingJobStatus.Pending;
        job.StartedAt = null;
        job.FinishedAt = null;
        job.ErrorMessage = null;
        job.ModifiedAt = DateTime.UtcNow;
        job.ModifiedBy = userId;
        await _processingJobRepository.UpdateAsync(job);
        await _processingJobRepository.SaveChangesAsync();
        return job;
    }

    public Task<List<ProcessingJob>> GetAllAsync(string userId)
    {
        return _processingJobRepository.GetAllAsync<ProcessingJob>(
            selector: x => x,
            predicate: x => x.Document.OwnerId == userId,
            orderBy: x => x.OrderByDescending(job => job.CreatedAt));
    }

    public async Task<PaginatedResult<ProcessingJob>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException(
                "Page number must be greater than or equal to 1.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentException(
                "Page size must be greater than zero.");
        }

        return await _processingJobRepository.GetAllPagedAsync<ProcessingJob>(
            selector: x => x,
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: x => x.Document.OwnerId == userId,
            orderBy: x => x.OrderByDescending(job => job.CreatedAt),
            asNoTracking: true);
    }

    public async Task<ProcessingJob> UpdateAsync(
        Guid id, UpdateProcessingJobDto dto, string userId)
    {
        var processingJob = await _processingJobRepository
            .GetAsync<ProcessingJob>(
                selector: x => x,
                predicate: x => x.Id == id && x.Document.OwnerId == userId);

        if (processingJob == null)
        {
            throw new KeyNotFoundException(
                $"Processing job with ID '{id}' was not found.");
        }

        processingJob.Status = dto.Status;
        processingJob.StartedAt = dto.StartedAt;
        processingJob.FinishedAt = dto.FinishedAt;
        processingJob.ErrorMessage = dto.ErrorMessage;
        processingJob.ModifiedAt = DateTime.UtcNow;
        processingJob.ModifiedBy = userId;

        await _processingJobRepository.UpdateAsync(processingJob);
        await _processingJobRepository.SaveChangesAsync();

        return processingJob;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var processingJob = await _processingJobRepository
            .GetAsync<ProcessingJob>(
                selector: x => x,
                predicate: x => x.Id == id && x.Document.OwnerId == userId);

        if (processingJob == null)
        {
            return false;
        }

        await _processingJobRepository.DeleteAsync(processingJob);
        await _processingJobRepository.SaveChangesAsync();

        return true;
    }
}
