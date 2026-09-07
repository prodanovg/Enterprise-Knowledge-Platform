using System.Security.Claims;
using Domain.Dto;
using Domain.Dto.Documents;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Xunit;

namespace Tests.Document;

public class DocumentControllerTests
{
    private readonly Mock<IDocumentService>
        _documentServiceMock;

    private readonly DocumentController _controller;

    public DocumentControllerTests()
    {
        _documentServiceMock =
            new Mock<IDocumentService>();

        _controller =
            new DocumentController(
                _documentServiceMock.Object);
    }

    private void SetUser(string userId)
    {
        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                userId)
        };

        var identity =
            new ClaimsIdentity(
                claims,
                "TestAuthentication");

        var user =
            new ClaimsPrincipal(identity);

        _controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User = user
                    }
            };
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        var userId = "user-id";

        SetUser(userId);

        var dto = new CreateDocumentDto
        {
            Name = "Test Document",
            FilePath = "/files/test.pdf",
            FileType = "application/pdf"
        };

        var documentDto = new DocumentDto
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            FilePath = dto.FilePath,
            FileType = dto.FileType,
            Status = DocumentStatus.Pending
        };

        _documentServiceMock
            .Setup(x => x.CreateAsync(
                dto,
                userId))
            .ReturnsAsync(documentDto);

        var result = await _controller.Create(dto);

        var createdResult =
            Assert.IsType<
                CreatedAtActionResult>(
                result.Result);

        Assert.Equal(
            nameof(DocumentController.GetById),
            createdResult.ActionName);

        Assert.Equal(
            documentDto,
            createdResult.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserIdDoesNotExist()
    {
        _controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext()
            };

        var dto = new CreateDocumentDto
        {
            Name = "Test Document",
            FilePath = "/files/test.pdf",
            FileType = "application/pdf"
        };

        var result =
            await _controller.Create(dto);

        Assert.IsType<
            UnauthorizedResult>(
            result.Result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenDocumentExists()
    {
        var userId = "user-id";
        var documentId = Guid.NewGuid();

        SetUser(userId);

        var documentDto = new DocumentDto
        {
            Id = documentId,
            Name = "Test Document",
            FilePath = "/files/test.pdf",
            FileType = "application/pdf",
            Status = DocumentStatus.Pending
        };

        _documentServiceMock
            .Setup(x => x.GetByIdAsync(
                documentId,
                userId))
            .ReturnsAsync(documentDto);

        var result =
            await _controller.GetById(documentId);

        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        Assert.Equal(
            documentDto,
            okResult.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        var userId = "user-id";
        var documentId = Guid.NewGuid();

        SetUser(userId);

        _documentServiceMock
            .Setup(x => x.GetByIdAsync(
                documentId,
                userId))
            .ReturnsAsync(
                (DocumentDto?)null);

        var result =
            await _controller.GetById(documentId);

        Assert.IsType<NotFoundResult>(
            result.Result);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        var userId = "user-id";

        SetUser(userId);

        var documents =
            new List<DocumentDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Document 1",
                    FilePath = "/files/1.pdf",
                    FileType = "application/pdf",
                    Status =
                        DocumentStatus.Pending
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Document 2",
                    FilePath = "/files/2.pdf",
                    FileType = "application/pdf",
                    Status =
                        DocumentStatus.Processed
                }
            };

        _documentServiceMock
            .Setup(x => x.GetAllAsync(userId))
            .ReturnsAsync(documents);

        var result =
            await _controller.GetAll();

        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        Assert.Equal(
            documents,
            okResult.Value);
    }

    [Fact]
    public async Task GetAllPaged_ShouldReturnBadRequest_WhenPageNumberIsInvalid()
    {
        var result =
            await _controller.GetAllPaged(
                0,
                10);

        Assert.IsType<BadRequestObjectResult>(
            result.Result);
    }

    [Fact]
    public async Task GetAllPaged_ShouldReturnBadRequest_WhenPageSizeIsInvalid()
    {
        var result =
            await _controller.GetAllPaged(
                1,
                0);

        Assert.IsType<BadRequestObjectResult>(
            result.Result);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenDocumentExists()
    {
        var userId = "user-id";
        var documentId = Guid.NewGuid();

        SetUser(userId);

        var dto = new UpdateDocumentDto
        {
            Name = "Updated Document",
            Status =
                DocumentStatus.Processed
        };

        var documentDto = new DocumentDto
        {
            Id = documentId,
            Name = "Updated Document",
            Status =
                DocumentStatus.Processed
        };

        _documentServiceMock
            .Setup(x => x.UpdateAsync(
                documentId,
                dto,
                userId))
            .ReturnsAsync(documentDto);

        var result =
            await _controller.Update(
                documentId,
                dto);

        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        Assert.Equal(
            documentDto,
            okResult.Value);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        var userId = "user-id";
        var documentId = Guid.NewGuid();

        SetUser(userId);

        var dto = new UpdateDocumentDto
        {
            Name = "Updated Document",
            Status =
                DocumentStatus.Processed
        };

        _documentServiceMock
            .Setup(x => x.UpdateAsync(
                documentId,
                dto,
                userId))
            .ThrowsAsync(
                new KeyNotFoundException());

        var result =
            await _controller.Update(
                documentId,
                dto);

        Assert.IsType<NotFoundResult>(
            result.Result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenDocumentIsDeleted()
    {
        var userId = "user-id";
        var documentId = Guid.NewGuid();

        SetUser(userId);

        _documentServiceMock
            .Setup(x => x.DeleteAsync(
                documentId,
                userId))
            .ReturnsAsync(true);

        var result =
            await _controller.Delete(
                documentId);

        Assert.IsType<NoContentResult>(
            result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        var userId = "user-id";
        var documentId = Guid.NewGuid();

        SetUser(userId);

        _documentServiceMock
            .Setup(x => x.DeleteAsync(
                documentId,
                userId))
            .ReturnsAsync(false);

        var result =
            await _controller.Delete(
                documentId);

        Assert.IsType<NotFoundResult>(
            result);
    }
}