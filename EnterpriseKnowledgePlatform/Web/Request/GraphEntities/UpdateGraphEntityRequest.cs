namespace Web.Request.GraphEntities;

public class UpdateGraphEntityRequest
{
    public string Name { get; set; } = string.Empty;
    public string CanonicalName { get; set; } = string.Empty;
    public Guid EntityTypeId { get; set; }
}
