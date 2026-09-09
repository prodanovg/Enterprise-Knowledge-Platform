namespace Domain.Dto.GraphRelationships;

public class UpdateGraphRelationshipDto
{
    public Guid SourceEntityId { get; set; }
    public Guid TargetEntityId { get; set; }
    public string Predicate { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
}
