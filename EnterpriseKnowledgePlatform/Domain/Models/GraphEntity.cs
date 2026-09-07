using Domain.Common;

namespace Domain.Models;

public class GraphEntity : BaseAuditableEntity<string>
{
    public string Name { get; set; } = string.Empty;
    public string CanonicalName { get; set; } = string.Empty;

    public Guid EntityTypeId { get; set; }

    public EntityType EntityType { get; set; } = null!;

    public ICollection<GraphRelationship> OutgoingRelationships { get; set; } =
        new List<GraphRelationship>();

    public ICollection<GraphRelationship> IncomingRelationships { get; set; } =
        new List<GraphRelationship>();
}