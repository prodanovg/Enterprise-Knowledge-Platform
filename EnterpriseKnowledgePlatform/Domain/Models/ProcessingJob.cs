using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class ProcessingJob : BaseAuditableEntity<string>
{
    public Guid DocumentId { get; set; }
    public ProcessingJobStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public Document Document { get; set; } = null!;
}