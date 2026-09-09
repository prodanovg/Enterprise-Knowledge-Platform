namespace Web.Response.EntityTypes;

public class EntityTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
