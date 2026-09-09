namespace Domain.Dto.TripleProvenance;

public class TripleProvenanceDto
{
    public Guid Id { get; set; }

    public Guid TripleId { get; set; }

    public Guid DocumentId { get; set; }

    public Guid SemanticBlockId { get; set; }

    public DateTime CreatedAt { get; set; }
}
