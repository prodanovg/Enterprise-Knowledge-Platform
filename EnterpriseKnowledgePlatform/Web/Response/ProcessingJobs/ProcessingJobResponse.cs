using Domain.Enums;

namespace Web.Response.ProcessingJobs;

public class ProcessingJobResponse
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public ProcessingJobStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
