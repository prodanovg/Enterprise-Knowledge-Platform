using System.Security.Claims;
using Domain.Dto;
using Domain.Dto.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.Notifications;
using Xunit;
using NotificationEntity = Domain.Models.Notification;

namespace Tests.Notification;

public class NotificationControllerTests
{
    private readonly Mock<INotificationService> _service = new();
    private readonly NotificationController _controller;

    public NotificationControllerTests() => _controller = new(new NotificationMapper(_service.Object));

    [Fact]
    public async Task CreateAndGetById_ShouldReturnExpectedResults()
    {
        SetUser();
        var notification = NewNotification();
        _service.Setup(x => x.CreateAsync(It.IsAny<CreateNotificationDto>(), "user-id")).ReturnsAsync(notification);
        Assert.IsType<CreatedAtActionResult>((await _controller.Create(new CreateNotificationRequest())).Result);
        _service.Setup(x => x.GetByIdAsync(notification.Id, "user-id")).ReturnsAsync(notification);
        Assert.IsType<OkObjectResult>((await _controller.GetById(notification.Id)).Result);
        _service.Setup(x => x.GetByIdAsync(notification.Id, "user-id")).ReturnsAsync((NotificationEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(notification.Id)).Result);
    }

    [Fact]
    public async Task GetAllAndPaged_ShouldHandleAuthorizationAndPaging()
    {
        SetUser();
        _service.Setup(x => x.GetAllAsync("user-id")).ReturnsAsync(new List<NotificationEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAll()).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(0, 10)).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(1, 0)).Result);
        _service.Setup(x => x.GetAllPagedAsync(1, 10, "user-id")).ReturnsAsync(new PaginatedResult<NotificationEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);
        SetUnauthenticated();
        Assert.IsType<UnauthorizedResult>((await _controller.GetAll()).Result);
    }

    [Fact]
    public async Task UpdateAndDelete_ShouldReturnExpectedResults()
    {
        SetUser();
        var id = Guid.NewGuid();
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateNotificationDto>(), "user-id")).ReturnsAsync(NewNotification());
        Assert.IsType<OkObjectResult>((await _controller.Update(id, new UpdateNotificationRequest())).Result);
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateNotificationDto>(), "user-id")).ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>((await _controller.Update(id, new UpdateNotificationRequest())).Result);
        _service.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(true);
        Assert.IsType<NoContentResult>(await _controller.Delete(id));
        _service.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(false);
        Assert.IsType<NotFoundResult>(await _controller.Delete(id));
        SetUnauthenticated();
        Assert.IsType<UnauthorizedResult>(await _controller.Delete(id));
    }

    private void SetUser() => _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-id") }, "Test")) } };
    private void SetUnauthenticated() => _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    private static NotificationEntity NewNotification() => new() { Id = Guid.NewGuid(), UserId = "user-id", Title = "Title", Message = "Message" };
}
