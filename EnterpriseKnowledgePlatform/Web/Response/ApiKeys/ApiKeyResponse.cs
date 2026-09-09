namespace Web.Response.ApiKeys;

public class ApiKeyResponse
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
