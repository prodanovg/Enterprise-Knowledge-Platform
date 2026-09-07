using Domain.Common;

namespace Domain.Models;

public class Notification : BaseAuditableEntity<string>
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
}
