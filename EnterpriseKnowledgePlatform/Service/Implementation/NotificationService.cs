using Domain.Dto;
using Domain.Dto.Notifications;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notificationRepository;

    public NotificationService(IRepository<Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Notification> CreateAsync(
        CreateNotificationDto dto, string userId)
    {
        var now = DateTime.UtcNow;
        var notification = new Notification
        {
            UserId = userId,
            Title = dto.Title,
            Message = dto.Message,
            SentAt = dto.SentAt,
            CreatedAt = now,
            CreatedBy = userId,
            ModifiedAt = now,
            ModifiedBy = userId
        };
        await _notificationRepository.InsertAsync(notification);
        await _notificationRepository.SaveChangesAsync();
        return notification;
    }

    public Task<Notification?> GetByIdAsync(Guid id, string userId) =>
        _notificationRepository.GetAsync<Notification>(x => x,
            x => x.Id == id && x.UserId == userId, asNoTracking: true);

    public Task<List<Notification>> GetAllAsync(string userId) =>
        _notificationRepository.GetAllAsync<Notification>(x => x,
            x => x.UserId == userId,
            x => x.OrderByDescending(notification => notification.CreatedAt));

    public async Task<PaginatedResult<Notification>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId)
    {
        ValidatePaging(pageNumber, pageSize);
        return await _notificationRepository.GetAllPagedAsync<Notification>(
            x => x, pageNumber, pageSize,
            x => x.UserId == userId,
            x => x.OrderByDescending(notification => notification.CreatedAt),
            asNoTracking: true);
    }

    public async Task<Notification> UpdateAsync(
        Guid id, UpdateNotificationDto dto, string userId)
    {
        var notification = await _notificationRepository.GetAsync<Notification>(
            x => x, x => x.Id == id && x.UserId == userId);
        if (notification == null)
        {
            throw new KeyNotFoundException(
                $"Notification with ID '{id}' was not found.");
        }

        notification.Title = dto.Title;
        notification.Message = dto.Message;
        notification.SentAt = dto.SentAt;
        notification.ModifiedAt = DateTime.UtcNow;
        notification.ModifiedBy = userId;
        await _notificationRepository.UpdateAsync(notification);
        await _notificationRepository.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var notification = await _notificationRepository.GetAsync<Notification>(
            x => x, x => x.Id == id && x.UserId == userId);
        if (notification == null) return false;

        await _notificationRepository.DeleteAsync(notification);
        await _notificationRepository.SaveChangesAsync();
        return true;
    }

    private static void ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than or equal to 1.");
        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.");
    }
}
