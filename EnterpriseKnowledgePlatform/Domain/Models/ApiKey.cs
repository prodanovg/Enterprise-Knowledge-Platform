using Domain.Common;

namespace Domain.Models;

public class ApiKey : BaseAuditableEntity<string>
{
    public Guid UserId { get; set; }
    public string KeyHash { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}
