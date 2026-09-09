namespace Domain.Dto.TripleProvenance;

public class UpdateTripleProvenanceDto
{
    public Guid TripleId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid SemanticBlockId { get; set; }
}
