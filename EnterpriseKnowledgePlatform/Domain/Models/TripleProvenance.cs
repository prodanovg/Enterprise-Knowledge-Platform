using Domain.Common;

namespace Domain.Models;

public class TripleProvenance : BaseAuditableEntity<string>
{
    public Guid TripleId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid SemanticBlockId { get; set; }
}
