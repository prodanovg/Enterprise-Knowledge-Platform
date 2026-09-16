using Domain.Models;
using Web.Response;

namespace Web.Clients;

public interface IProcessingApiClient
{
    Task<ProcessingApiResponse> SendProcessingJobAsync(
        ProcessingJob processingJob,
        Document document,
        CancellationToken cancellationToken = default);
}
