using Domain.Common;

namespace Domain.Models;

public class ApiKey : BaseAuditableEntity<string>
{
    public string UserId { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }

    public User User { get; set; } = null!;
}