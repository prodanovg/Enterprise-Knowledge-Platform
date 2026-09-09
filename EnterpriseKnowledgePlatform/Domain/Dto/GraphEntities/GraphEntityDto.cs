namespace Domain.Dto.GraphEntities;

public class GraphEntityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CanonicalName { get; set; } = string.Empty;
    public Guid EntityTypeId { get; set; }
}
