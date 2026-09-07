using Domain.Common;

namespace Domain.Models;

public class GraphRelationship : BaseAuditableEntity<string>
{
    public Guid SourceEntityId { get; set; }
    public Guid TargetEntityId { get; set; }

    public string Predicate { get; set; } = string.Empty;
    public decimal Confidence { get; set; }

    public GraphEntity SourceEntity { get; set; } = null!;
    public GraphEntity TargetEntity { get; set; } = null!;
}