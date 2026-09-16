using Service.Interface; using Web.Request.ProcessingJobs;
namespace Web.Mapper;
public class ProcessingResultMapper { private readonly IProcessingResultService _service; public ProcessingResultMapper(IProcessingResultService service) => _service = service; public Task<ProcessingResultSummary> PersistAsync(Guid id, ProcessingResultRequest request, CancellationToken token) => _service.PersistAsync(id, request, token); }
