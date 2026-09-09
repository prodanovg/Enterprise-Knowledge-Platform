using Domain.Dto.Notifications;
using Service.Interface;
using Web.Extensions;
using Web.Request.Notifications;
using Web.Response;
using Web.Response.Notifications;

namespace Web.Mapper;

public class NotificationMapper
{
    private readonly INotificationService _notificationService;

    public NotificationMapper(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<NotificationResponse> CreateAsync(
        CreateNotificationRequest request, string userId) =>
        (await _notificationService.CreateAsync(ToDto(request), userId)).ToResponse();

    public async Task<NotificationResponse?> GetByIdAsync(Guid id, string userId) =>
        (await _notificationService.GetByIdAsync(id, userId))?.ToResponse();

    public async Task<List<NotificationResponse>> GetAllAsync(string userId) =>
        (await _notificationService.GetAllAsync(userId)).ToResponse();

    public async Task<PaginatedResponse<NotificationResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId) =>
        (await _notificationService.GetAllPagedAsync(pageNumber, pageSize, userId)).ToResponse();

    public async Task<NotificationResponse> UpdateAsync(
        Guid id, UpdateNotificationRequest request, string userId) =>
        (await _notificationService.UpdateAsync(id, ToDto(request), userId)).ToResponse();

    public Task<bool> DeleteAsync(Guid id, string userId) =>
        _notificationService.DeleteAsync(id, userId);

    public static CreateNotificationDto ToDto(CreateNotificationRequest request) => new()
    { Title = request.Title, Message = request.Message, SentAt = request.SentAt };

    public static UpdateNotificationDto ToDto(UpdateNotificationRequest request) => new()
    { Title = request.Title, Message = request.Message, SentAt = request.SentAt };
}
