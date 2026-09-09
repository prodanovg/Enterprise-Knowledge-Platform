using Domain.Models;

namespace Web.Clients;

public interface IProcessingApiClient
{
    Task SendProcessingJobAsync(
        ProcessingJob processingJob,
        Document document,
        CancellationToken cancellationToken = default);
}
