using Domain.Common;

namespace Domain.Models;

public class TripleProvenance : BaseAuditableEntity<string>
{
    public Guid TripleId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid SemanticBlockId { get; set; }

    public Triple Triple { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public SemanticBlock SemanticBlock { get; set; } = null!;
}