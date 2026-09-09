namespace Web.Response.GraphRelationships;

public class GraphRelationshipResponse
{
    public Guid Id { get; set; }
    public Guid SourceEntityId { get; set; }
    public Guid TargetEntityId { get; set; }
    public string Predicate { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public DateTime CreatedAt { get; set; }
}
