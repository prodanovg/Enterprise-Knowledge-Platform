namespace Web.Response.GraphEntities;

public class GraphEntityResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CanonicalName { get; set; } = string.Empty;
    public Guid EntityTypeId { get; set; }
    public DateTime CreatedAt { get; set; }
}
