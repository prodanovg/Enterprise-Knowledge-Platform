using Domain.Dto.ProcessingResults;

namespace Service.Interface;

public interface IProcessingResultService
{
    Task<ProcessingResultSummary> PersistAsync(
        Guid processingJobId,
        ProcessingResultDto result,
        CancellationToken cancellationToken = default);
}

public class ProcessingResultSummary
{
    public Guid ProcessingJobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SemanticBlocks { get; set; }
    public int Triples { get; set; }
}
