using System.Security.Claims;
using Domain.Dto;
using Domain.Dto.TripleProvenance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.TripleProvenance;
using Xunit;
using ProvenanceEntity = Domain.Models.TripleProvenance;

namespace Tests.TripleProvenance;

public class TripleProvenanceControllerTests
{
    private readonly Mock<ITripleProvenanceService> _serviceMock;
    private readonly TripleProvenanceController _controller;

    public TripleProvenanceControllerTests()
    {
        _serviceMock = new Mock<ITripleProvenanceService>();
        _controller = new TripleProvenanceController(
            new TripleProvenanceMapper(_serviceMock.Object));
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        SetUser("user-id");
        var request = CreateRequest();
        _serviceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateTripleProvenanceDto>(), "user-id"))
            .ReturnsAsync(new ProvenanceEntity { Id = Guid.NewGuid() });

        var result = await _controller.Create(request);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorizedOrNotFound()
    {
        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>(
            (await _controller.Create(CreateRequest())).Result);

        SetUser("user-id");
        _serviceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreateTripleProvenanceDto>(), "user-id"))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>(
            (await _controller.Create(CreateRequest())).Result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkOrNotFound()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.GetByIdAsync(id, "user-id"))
            .ReturnsAsync(new ProvenanceEntity { Id = id });
        Assert.IsType<OkObjectResult>((await _controller.GetById(id)).Result);

        _serviceMock.Setup(x => x.GetByIdAsync(id, "user-id"))
            .ReturnsAsync((ProvenanceEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(id)).Result);
    }

    [Fact]
    public async Task GetById_ShouldReturnUnauthorized_WhenUserIdDoesNotExist()
    {
        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>(
            (await _controller.GetById(Guid.NewGuid())).Result);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkOrUnauthorized()
    {
        SetUser("user-id");
        _serviceMock.Setup(x => x.GetAllAsync("user-id"))
            .ReturnsAsync(new List<ProvenanceEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAll()).Result);

        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>((await _controller.GetAll()).Result);
    }

    [Fact]
    public async Task GetAllPaged_ShouldValidateAndReturnOkOrUnauthorized()
    {
        Assert.IsType<BadRequestObjectResult>(
            (await _controller.GetAllPaged(0, 10)).Result);
        Assert.IsType<BadRequestObjectResult>(
            (await _controller.GetAllPaged(1, 0)).Result);

        SetUser("user-id");
        _serviceMock.Setup(x => x.GetAllPagedAsync(1, 10, "user-id"))
            .ReturnsAsync(new PaginatedResult<ProvenanceEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);

        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>((await _controller.GetAllPaged()).Result);
    }

    [Fact]
    public async Task Update_ShouldReturnOkOrNotFoundOrUnauthorized()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        var request = new UpdateTripleProvenanceRequest();
        _serviceMock.Setup(x => x.UpdateAsync(
                id, It.IsAny<UpdateTripleProvenanceDto>(), "user-id"))
            .ReturnsAsync(new ProvenanceEntity { Id = id });
        Assert.IsType<OkObjectResult>((await _controller.Update(id, request)).Result);

        _serviceMock.Setup(x => x.UpdateAsync(
                id, It.IsAny<UpdateTripleProvenanceDto>(), "user-id"))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>((await _controller.Update(id, request)).Result);

        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>((await _controller.Update(id, request)).Result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContentOrNotFoundOrUnauthorized()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(true);
        Assert.IsType<NoContentResult>(await _controller.Delete(id));

        _serviceMock.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(false);
        Assert.IsType<NotFoundResult>(await _controller.Delete(id));

        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>(await _controller.Delete(id));
    }

    private static CreateTripleProvenanceRequest CreateRequest()
    {
        return new CreateTripleProvenanceRequest
        {
            TripleId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            SemanticBlockId = Guid.NewGuid()
        };
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
