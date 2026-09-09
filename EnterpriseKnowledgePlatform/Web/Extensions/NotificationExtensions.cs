using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.Notifications;

namespace Web.Extensions;

public static class NotificationExtensions
{
    public static NotificationResponse ToResponse(this Notification notification) => new()
    {
        Id = notification.Id,
        Title = notification.Title,
        Message = notification.Message,
        SentAt = notification.SentAt,
        CreatedAt = notification.CreatedAt
    };

    public static List<NotificationResponse> ToResponse(
        this IEnumerable<Notification> notifications) =>
        notifications.Select(x => x.ToResponse()).ToList();

    public static PaginatedResponse<NotificationResponse> ToResponse(
        this PaginatedResult<Notification> result) => new()
    {
        Items = result.Items.ToResponse(),
        TotalCount = result.TotalCount,
        PageNumber = result.PageNumber,
        PageSize = result.PageSize,
        TotalPages = result.TotalPages
    };
}
