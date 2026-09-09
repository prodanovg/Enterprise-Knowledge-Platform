namespace Web.Request.ProcessingJobs;

public class StartProcessingRequest
{
    public List<Guid> DocumentIds { get; set; } = new();
}
