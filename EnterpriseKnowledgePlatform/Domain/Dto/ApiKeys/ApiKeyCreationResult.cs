using Domain.Models;

namespace Domain.Dto.ApiKeys;

public class ApiKeyCreationResult
{
    public ApiKey ApiKey { get; set; } = null!;
    public string PlaintextKey { get; set; } = string.Empty;
}
