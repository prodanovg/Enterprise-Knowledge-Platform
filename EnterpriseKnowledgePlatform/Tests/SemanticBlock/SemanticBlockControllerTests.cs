using System.Security.Claims;
using Domain.Dto;
using Domain.Dto.SemanticBlocks;
using SemanticBlockEntity = Domain.Models.SemanticBlock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.SemanticBlocks;
using Xunit;

namespace Tests.SemanticBlock;

public class SemanticBlockControllerTests
{
    private readonly Mock<ISemanticBlockService> _serviceMock;
    private readonly SemanticBlockController _controller;

    public SemanticBlockControllerTests()
    {
        _serviceMock = new Mock<ISemanticBlockService>();
        _controller = new SemanticBlockController(new SemanticBlockMapper(_serviceMock.Object));
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        SetUser("user-id");
        var request = new CreateSemanticBlockRequest
        {
            DocumentId = Guid.NewGuid(), Text = "Text"
        };
        _serviceMock.Setup(x => x.CreateAsync(
                It.IsAny<CreateSemanticBlockDto>(), "user-id"))
            .ReturnsAsync(new SemanticBlockEntity
            {
                Id = Guid.NewGuid(), DocumentId = request.DocumentId
            });

        Assert.IsType<CreatedAtActionResult>(
            (await _controller.Create(request)).Result);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorizedOrNotFound()
    {
        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>(
            (await _controller.Create(new CreateSemanticBlockRequest())).Result);

        SetUser("user-id");
        _serviceMock.Setup(x => x.CreateAsync(
                It.IsAny<CreateSemanticBlockDto>(), "user-id"))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>(
            (await _controller.Create(new CreateSemanticBlockRequest())).Result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkOrNotFound()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.GetByIdAsync(id, "user-id"))
            .ReturnsAsync(new SemanticBlockEntity { Id = id });
        Assert.IsType<OkObjectResult>((await _controller.GetById(id)).Result);

        _serviceMock.Setup(x => x.GetByIdAsync(id, "user-id"))
            .ReturnsAsync((SemanticBlockEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(id)).Result);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        SetUser("user-id");
        _serviceMock.Setup(x => x.GetAllAsync("user-id"))
            .ReturnsAsync(new List<SemanticBlockEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAll()).Result);
    }

    [Fact]
    public async Task GetAllPaged_ShouldValidateAndReturnOk()
    {
        Assert.IsType<BadRequestObjectResult>(
            (await _controller.GetAllPaged(0, 10)).Result);
        Assert.IsType<BadRequestObjectResult>(
            (await _controller.GetAllPaged(1, 0)).Result);

        SetUser("user-id");
        _serviceMock.Setup(x => x.GetAllPagedAsync(1, 10, "user-id"))
            .ReturnsAsync(new PaginatedResult<SemanticBlockEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);
    }

    [Fact]
    public async Task Update_ShouldReturnOkOrNotFound()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        var request = new UpdateSemanticBlockRequest();
        _serviceMock.Setup(x => x.UpdateAsync(
                id, It.IsAny<UpdateSemanticBlockDto>(), "user-id"))
            .ReturnsAsync(new SemanticBlockEntity { Id = id });
        Assert.IsType<OkObjectResult>((await _controller.Update(id, request)).Result);

        _serviceMock.Setup(x => x.UpdateAsync(
                id, It.IsAny<UpdateSemanticBlockDto>(), "user-id"))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>((await _controller.Update(id, request)).Result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContentOrNotFound()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(true);
        Assert.IsType<NoContentResult>(await _controller.Delete(id));
        _serviceMock.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(false);
        Assert.IsType<NotFoundResult>(await _controller.Delete(id));
    }

    private void SetUser(string userId)
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
                    "TestAuthentication"))
            }
        };
    }

    private void SetUnauthenticatedUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
}

