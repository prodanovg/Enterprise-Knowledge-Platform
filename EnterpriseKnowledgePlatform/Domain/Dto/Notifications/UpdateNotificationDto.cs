namespace Domain.Dto.Notifications;

public class UpdateNotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
}
