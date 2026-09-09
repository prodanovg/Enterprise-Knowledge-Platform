namespace Web.Request.TripleProvenance;

public class CreateTripleProvenanceRequest
{
    public Guid TripleId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid SemanticBlockId { get; set; }
}
