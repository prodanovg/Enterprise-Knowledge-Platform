using Domain.Common;

namespace Domain.Models;

public class Notification : BaseAuditableEntity<string>
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }

    public User User { get; set; } = null!;
}