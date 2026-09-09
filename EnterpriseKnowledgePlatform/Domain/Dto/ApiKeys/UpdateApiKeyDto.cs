namespace Domain.Dto.ApiKeys;

public class UpdateApiKeyDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}
