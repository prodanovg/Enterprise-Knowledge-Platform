namespace Domain.Dto.ApiKeys;

public class CreateApiKeyDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}
