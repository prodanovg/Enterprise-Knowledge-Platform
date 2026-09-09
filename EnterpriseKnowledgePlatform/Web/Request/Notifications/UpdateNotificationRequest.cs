namespace Web.Request.Notifications;

public class UpdateNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
}
