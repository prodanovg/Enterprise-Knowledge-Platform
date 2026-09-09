using System.Security.Claims;
using Domain.Dto.Documents;
using Domain.Enums;
using DocumentEntity = Domain.Models.Document;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.Documents;
using Xunit;

namespace Tests.Document;

public class DocumentControllerTests
{
    private readonly Mock<IDocumentService> _serviceMock;
    private readonly DocumentController _controller;

    public DocumentControllerTests()
    {
        _serviceMock = new Mock<IDocumentService>();
        _controller = new DocumentController(new DocumentMapper(_serviceMock.Object));
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        SetUser("user-id");
        var request = new CreateDocumentRequest { Name = "Document" };
        _serviceMock.Setup(x => x.CreateAsync(It.IsAny<CreateDocumentDto>(), "user-id"))
            .ReturnsAsync(new DocumentEntity { Id = Guid.NewGuid(), Name = "Document" });

        var result = await _controller.Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(DocumentController.GetById), created.ActionName);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserIdDoesNotExist()
    {
        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>(
            (await _controller.Create(new CreateDocumentRequest())).Result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkOrNotFound()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.GetByIdAsync(id, "user-id"))
            .ReturnsAsync(new DocumentEntity { Id = id });
        Assert.IsType<OkObjectResult>((await _controller.GetById(id)).Result);

        _serviceMock.Setup(x => x.GetByIdAsync(id, "user-id"))
            .ReturnsAsync((DocumentEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(id)).Result);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        SetUser("user-id");
        _serviceMock.Setup(x => x.GetAllAsync("user-id"))
            .ReturnsAsync(new List<DocumentEntity>());

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
            .ReturnsAsync(new Domain.Dto.PaginatedResult<DocumentEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);
    }

    [Fact]
    public async Task Update_ShouldReturnOkOrNotFound()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        var request = new UpdateDocumentRequest { Name = "Updated" };
        _serviceMock.Setup(x => x.UpdateAsync(
                id, It.IsAny<UpdateDocumentDto>(), "user-id"))
            .ReturnsAsync(new DocumentEntity { Id = id, Name = "Updated" });
        Assert.IsType<OkObjectResult>((await _controller.Update(id, request)).Result);

        _serviceMock.Setup(x => x.UpdateAsync(
                id, It.IsAny<UpdateDocumentDto>(), "user-id"))
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

