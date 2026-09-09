using Domain.Enums;

namespace Domain.Dto.ProcessingJobs;

public class UpdateProcessingJobDto
{
    public ProcessingJobStatus Status { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? ErrorMessage { get; set; }
}