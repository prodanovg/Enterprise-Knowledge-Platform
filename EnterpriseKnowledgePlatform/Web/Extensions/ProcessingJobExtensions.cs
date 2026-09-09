using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.ProcessingJobs;

namespace Web.Extensions;

public static class ProcessingJobExtensions
{
    public static ProcessingJobResponse ToResponse(
        this ProcessingJob processingJob)
    {
        return new ProcessingJobResponse
        {
            Id = processingJob.Id,
            DocumentId = processingJob.DocumentId,
            Status = processingJob.Status,
            StartedAt = processingJob.StartedAt,
            FinishedAt = processingJob.FinishedAt,
            ErrorMessage = processingJob.ErrorMessage,
            CreatedAt = processingJob.CreatedAt
        };
    }

    public static List<ProcessingJobResponse> ToResponse(
        this IEnumerable<ProcessingJob> processingJobs)
    {
        return processingJobs
            .Select(processingJob => processingJob.ToResponse())
            .ToList();
    }

    public static PaginatedResponse<ProcessingJobResponse> ToResponse(
        this PaginatedResult<ProcessingJob> result)
    {
        return new PaginatedResponse<ProcessingJobResponse>
        {
            Items = result.Items.ToResponse(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }
}
