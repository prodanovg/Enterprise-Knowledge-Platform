using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.Notifications;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using NotificationEntity = Domain.Models.Notification;

namespace Tests.Notification;

public class NotificationServiceTests
{
    private readonly Mock<IRepository<NotificationEntity>> _repository = new();
    private readonly NotificationService _service;

    public NotificationServiceTests() => _service = new(_repository.Object);

    [Fact]
    public async Task CreateAsync_ShouldAssignUserAndCreate()
    {
        _repository.Setup(x => x.InsertAsync(It.IsAny<NotificationEntity>())).ReturnsAsync((NotificationEntity x) => x);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        var result = await _service.CreateAsync(new CreateNotificationDto { Title = "Title", Message = "Message" }, "user-id");
        Assert.Equal("user-id", result.UserId);
        Assert.Equal("Title", result.Title);
        Assert.Equal("user-id", result.CreatedBy);
    }

    [Fact]
    public async Task GetAndPagedAsync_ShouldApplyOwnershipAndPaging()
    {
        var notification = NewNotification("user-id");
        SetupGet(notification);
        Assert.Same(notification, await _service.GetByIdAsync(notification.Id, "user-id"));
        SetupGet(null);
        Assert.Null(await _service.GetByIdAsync(Guid.NewGuid(), "user-id"));
        var items = new List<NotificationEntity> { notification };
        _repository.Setup(x => x.GetAllAsync<NotificationEntity>(It.IsAny<Expression<Func<NotificationEntity, NotificationEntity>>>(), It.IsAny<Expression<Func<NotificationEntity, bool>>>(), It.IsAny<Func<IQueryable<NotificationEntity>, IOrderedQueryable<NotificationEntity>>>(), null, null)).ReturnsAsync(items);
        var page = new PaginatedResult<NotificationEntity> { Items = items, TotalCount = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        _repository.Setup(x => x.GetAllPagedAsync<NotificationEntity>(It.IsAny<Expression<Func<NotificationEntity, NotificationEntity>>>(), 1, 10, It.IsAny<Expression<Func<NotificationEntity, bool>>>(), It.IsAny<Func<IQueryable<NotificationEntity>, IOrderedQueryable<NotificationEntity>>>(), null, true)).ReturnsAsync(page);
        Assert.Same(items, await _service.GetAllAsync("user-id"));
        Assert.Same(page, await _service.GetAllPagedAsync(1, 10, "user-id"));
    }

    [Fact]
    public async Task PagingAndUpdateDelete_ShouldHandleExpectedCases()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(0, 10, "user-id"));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(1, 0, "user-id"));
        var notification = NewNotification("user-id");
        SetupGet(notification);
        _repository.Setup(x => x.UpdateAsync(notification)).ReturnsAsync(notification);
        _repository.Setup(x => x.DeleteAsync(notification)).ReturnsAsync(notification);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        var updated = await _service.UpdateAsync(notification.Id, new UpdateNotificationDto { Title = "Updated" }, "user-id");
        Assert.Equal("Updated", updated.Title);
        Assert.True(await _service.DeleteAsync(notification.Id, "user-id"));
        SetupGet(null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), new UpdateNotificationDto(), "user-id"));
        Assert.False(await _service.DeleteAsync(Guid.NewGuid(), "user-id"));
    }

    private void SetupGet(NotificationEntity? value) => _repository.Setup(x => x.GetAsync<NotificationEntity>(It.IsAny<Expression<Func<NotificationEntity, NotificationEntity>>>(), It.IsAny<Expression<Func<NotificationEntity, bool>>>(), null, null, It.IsAny<bool>())).ReturnsAsync(value);
    private static NotificationEntity NewNotification(string userId) => new() { Id = Guid.NewGuid(), UserId = userId, Title = "Title", Message = "Message" };
}
