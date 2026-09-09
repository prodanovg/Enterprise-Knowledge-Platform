using System.Security.Claims;
using Domain.Dto;
using Domain.Dto.ApiKeys;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.ApiKeys;
using Xunit;
using ApiKeyEntity = Domain.Models.ApiKey;

namespace Tests.ApiKey;

public class ApiKeyControllerTests
{
    private readonly Mock<IApiKeyService> _service = new();
    private readonly ApiKeyController _controller;

    public ApiKeyControllerTests() =>
        _controller = new ApiKeyController(new ApiKeyMapper(_service.Object));

    [Fact]
    public async Task Create_ShouldReturnCreatedOrUnauthorized()
    {
        SetUser("user-id");
        _service.Setup(x => x.CreateAsync(It.IsAny<CreateApiKeyDto>(), "user-id"))
            .ReturnsAsync(new ApiKeyCreationResult
            { ApiKey = NewKey(), PlaintextKey = "secret" });
        Assert.IsType<CreatedAtActionResult>(
            (await _controller.Create(new CreateApiKeyRequest())).Result);

        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>(
            (await _controller.Create(new CreateApiKeyRequest())).Result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkNotFoundOrUnauthorized()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        _service.Setup(x => x.GetByIdAsync(id, "user-id")).ReturnsAsync(NewKey());
        Assert.IsType<OkObjectResult>((await _controller.GetById(id)).Result);
        _service.Setup(x => x.GetByIdAsync(id, "user-id")).ReturnsAsync((ApiKeyEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(id)).Result);
        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>((await _controller.GetById(id)).Result);
    }

    [Fact]
    public async Task GetAllAndPaged_ShouldHandleResultsValidationAndUnauthorized()
    {
        SetUser("user-id");
        _service.Setup(x => x.GetAllAsync("user-id")).ReturnsAsync(new List<ApiKeyEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAll()).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(0, 10)).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(1, 0)).Result);
        _service.Setup(x => x.GetAllPagedAsync(1, 10, "user-id"))
            .ReturnsAsync(new PaginatedResult<ApiKeyEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);
        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>((await _controller.GetAll()).Result);
    }

    [Fact]
    public async Task UpdateAndDelete_ShouldReturnExpectedResults()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateApiKeyDto>(), "user-id"))
            .ReturnsAsync(NewKey());
        Assert.IsType<OkObjectResult>(
            (await _controller.Update(id, new UpdateApiKeyRequest())).Result);
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateApiKeyDto>(), "user-id"))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>(
            (await _controller.Update(id, new UpdateApiKeyRequest())).Result);
        _service.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(true);
        Assert.IsType<NoContentResult>(await _controller.Delete(id));
        _service.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(false);
        Assert.IsType<NotFoundResult>(await _controller.Delete(id));
        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>(await _controller.Delete(id));
    }

    private void SetUser(string id) => _controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, id) }, "Test"))
        }
    };

    private void SetUnauthenticatedUser() => _controller.ControllerContext =
        new ControllerContext { HttpContext = new DefaultHttpContext() };

    private static ApiKeyEntity NewKey() => new() { Id = Guid.NewGuid(), UserId = "user-id" };
}
