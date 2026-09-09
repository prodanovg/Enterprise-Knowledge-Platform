namespace Web.Request.ApiKeys;

public class UpdateApiKeyRequest
{
    public string Label { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}
