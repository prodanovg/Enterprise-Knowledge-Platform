using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;
using Web.Mapper;
using Web.Request.Users;
using Web.Response.Users;

namespace Tests.UserCrud;

public class UserControllerTests
{
    private readonly Mock<UserMapper> _mapper;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        var store = new Mock<IUserStore<Domain.Models.User>>();
        var manager = new Mock<UserManager<Domain.Models.User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _mapper = new Mock<UserMapper>(manager.Object, null!);
        _controller = new UserController(_mapper.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsUsers()
    {
        _mapper.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<UserResponse> { new() { Id = "1" } });
        var result = await _controller.GetAll();
        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsType<List<UserResponse>>(response.Value));
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _mapper.Setup(x => x.GetByIdAsync("missing")).ReturnsAsync((UserResponse?)null);
        var result = await _controller.GetById("missing");
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsUpdatedUser()
    {
        _mapper.Setup(x => x.UpdateAsync("1", It.IsAny<UpdateUserRequest>()))
            .ReturnsAsync(new UserResponse { Id = "1", Name = "Updated" });
        var result = await _controller.Update("1", new UpdateUserRequest { Name = "Updated", Email = "u@example.com" });
        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("Updated", Assert.IsType<UserResponse>(response.Value).Name);
    }

    [Fact]
    public async Task Delete_ReturnsConflict_WhenUserHasDependents()
    {
        _mapper.Setup(x => x.DeleteAsync("1"))
            .ThrowsAsync(new InvalidOperationException("dependent"));
        var result = await _controller.Delete("1");
        Assert.IsType<ConflictObjectResult>(result);
    }
}
