using Domain.Dto.ProcessingJobs;
using Service.Interface;
using Web.Extensions;
using Web.Request.ProcessingJobs;
using Web.Response;
using Web.Response.ProcessingJobs;

namespace Web.Mapper;

public class ProcessingJobMapper
{
    private readonly IProcessingJobService _processingJobService;

    public ProcessingJobMapper(IProcessingJobService processingJobService)
    {
        _processingJobService = processingJobService;
    }

    public async Task<ProcessingJobResponse> CreateAsync(
        CreateProcessingJobRequest request, string userId)
    {
        var processingJob = await _processingJobService
            .CreateAsync(ToDto(request), userId);
        return processingJob.ToResponse();
    }

    public async Task<ProcessingJobResponse?> GetByIdAsync(Guid id, string userId)
    {
        var processingJob = await _processingJobService.GetByIdAsync(id, userId);
        return processingJob?.ToResponse();
    }

    public async Task<List<ProcessingJobResponse>> GetAllAsync(string userId)
    {
        var processingJobs = await _processingJobService.GetAllAsync(userId);
        return processingJobs.ToResponse();
    }

    public async Task<PaginatedResponse<ProcessingJobResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId)
    {
        var processingJobs = await _processingJobService
            .GetAllPagedAsync(pageNumber, pageSize, userId);
        return processingJobs.ToResponse();
    }

    public async Task<ProcessingJobResponse> UpdateAsync(
        Guid id, UpdateProcessingJobRequest request, string userId)
    {
        var processingJob = await _processingJobService
            .UpdateAsync(id, ToDto(request), userId);
        return processingJob.ToResponse();
    }

    public Task<bool> DeleteAsync(Guid id, string userId)
    {
        return _processingJobService.DeleteAsync(id, userId);
    }

    public static CreateProcessingJobDto ToDto(
        CreateProcessingJobRequest request)
    {
        return new CreateProcessingJobDto
        {
            DocumentId = request.DocumentId
        };
    }

    public static UpdateProcessingJobDto ToDto(
        UpdateProcessingJobRequest request)
    {
        return new UpdateProcessingJobDto
        {
            Status = request.Status,
            StartedAt = request.StartedAt,
            FinishedAt = request.FinishedAt,
            ErrorMessage = request.ErrorMessage
        };
    }
}
