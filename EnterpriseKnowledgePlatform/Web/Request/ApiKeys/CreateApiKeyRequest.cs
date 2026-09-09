namespace Web.Request.ApiKeys;

public class CreateApiKeyRequest
{
    public string Label { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}
