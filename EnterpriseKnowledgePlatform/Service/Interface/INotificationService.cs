using Domain.Dto;
using Domain.Dto.Notifications;
using Domain.Models;

namespace Service.Interface;

public interface INotificationService
{
    Task<Notification> CreateAsync(CreateNotificationDto dto, string userId);
    Task<Notification?> GetByIdAsync(Guid id, string userId);
    Task<List<Notification>> GetAllAsync(string userId);
    Task<PaginatedResult<Notification>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId);
    Task<Notification> UpdateAsync(Guid id, UpdateNotificationDto dto, string userId);
    Task<bool> DeleteAsync(Guid id, string userId);
}
