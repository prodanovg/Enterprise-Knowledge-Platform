using Domain.Enums;

namespace Web.Request.ProcessingJobs;

public class UpdateProcessingJobRequest
{
    public ProcessingJobStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
