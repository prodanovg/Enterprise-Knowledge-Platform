using Domain.Dto;
using Domain.Enums;
using Domain.Models;
using Domain.Dto.Documents;
using Moq;
using Service.Interface;
using Web.Mapper;
using Web.Request.Documents;
using Xunit;
using DocumentEntity = Domain.Models.Document;

namespace Tests.Mapper;

public class DocumentMapperTests
{
    [Fact]
    public async Task CreateAsync_ShouldMapRequestForwardUserAndMapResponse()
    {
        var serviceMock = new Mock<IDocumentService>();
        var mapper = new DocumentMapper(serviceMock.Object);
        var request = new CreateDocumentRequest
        {
            Name = "DocumentEntity",
            FilePath = "/file.pdf",
            FileType = "application/pdf"
        };
        var document = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            FilePath = request.FilePath,
            FileType = request.FileType,
            Status = DocumentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        serviceMock.Setup(x => x.CreateAsync(It.Is<CreateDocumentDto>(dto =>
                dto.Name == request.Name &&
                dto.FilePath == request.FilePath &&
                dto.FileType == request.FileType), "user-id"))
            .ReturnsAsync(document);

        var response = await mapper.CreateAsync(request, "user-id");

        Assert.Equal(document.Id, response.Id);
        Assert.Equal(document.Name, response.Name);
        serviceMock.Verify(x => x.CreateAsync(It.IsAny<CreateDocumentDto>(), "user-id"),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldMapEntityResponse()
    {
        var serviceMock = new Mock<IDocumentService>();
        var mapper = new DocumentMapper(serviceMock.Object);
        var document = new DocumentEntity { Id = Guid.NewGuid(), Name = "DocumentEntity" };
        serviceMock.Setup(x => x.GetByIdAsync(document.Id, "user-id"))
            .ReturnsAsync(document);

        var response = await mapper.GetByIdAsync(document.Id, "user-id");

        Assert.Equal(document.Id, response!.Id);
        Assert.Equal(document.Name, response.Name);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldMapPaginatedEntityResponse()
    {
        var serviceMock = new Mock<IDocumentService>();
        var mapper = new DocumentMapper(serviceMock.Object);
        var page = new PaginatedResult<DocumentEntity>
        {
            Items = new List<DocumentEntity> { new() { Id = Guid.NewGuid() } },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10,
            TotalPages = 1
        };
        serviceMock.Setup(x => x.GetAllPagedAsync(1, 10, "user-id"))
            .ReturnsAsync(page);

        var response = await mapper.GetAllPagedAsync(1, 10, "user-id");

        Assert.Equal(page.TotalCount, response.TotalCount);
        Assert.Equal(page.Items[0].Id, response.Items[0].Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldMapRequestAndForwardUser()
    {
        var serviceMock = new Mock<IDocumentService>();
        var mapper = new DocumentMapper(serviceMock.Object);
        var id = Guid.NewGuid();
        var request = new UpdateDocumentRequest
        {
            Name = "Updated",
            Status = DocumentStatus.Processed
        };
        serviceMock.Setup(x => x.UpdateAsync(id, It.IsAny<
                Domain.Dto.Documents.UpdateDocumentDto>(), "user-id"))
            .ReturnsAsync(new DocumentEntity { Id = id, Name = request.Name });

        var response = await mapper.UpdateAsync(id, request, "user-id");

        Assert.Equal(id, response.Id);
        serviceMock.Verify(x => x.UpdateAsync(id, It.Is<
            Domain.Dto.Documents.UpdateDocumentDto>(dto =>
                dto.Name == request.Name && dto.Status == request.Status),
            "user-id"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldForwardUser()
    {
        var serviceMock = new Mock<IDocumentService>();
        var mapper = new DocumentMapper(serviceMock.Object);
        var id = Guid.NewGuid();
        serviceMock.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(true);

        Assert.True(await mapper.DeleteAsync(id, "user-id"));
        serviceMock.Verify(x => x.DeleteAsync(id, "user-id"), Times.Once);
    }
}





