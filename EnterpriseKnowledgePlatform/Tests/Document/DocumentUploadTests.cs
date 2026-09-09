using Microsoft.AspNetCore.Http;
using Moq;
using Service.Interface;
using Web.Mapper;
using Web.Request.Documents;
using Web.Services;
using Xunit;
using DocumentEntity = Domain.Models.Document;

namespace Tests.Document;

public class DocumentUploadTests
{
    [Fact]
    public async Task CreateAsync_ShouldStoreGeneratedReferenceAndPreserveOwner()
    {
        var service = new Mock<IDocumentService>();
        var storage = new Mock<IFileStorageService>();
        storage.Setup(x => x.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("generated-guid.pdf");
        service.Setup(x => x.CreateAsync(It.IsAny<Domain.Dto.Documents.CreateDocumentDto>(), "user-id"))
            .ReturnsAsync((Domain.Dto.Documents.CreateDocumentDto dto, string _) => new DocumentEntity
            {
                Id = Guid.NewGuid(), Name = dto.Name, FilePath = dto.FilePath,
                FileType = dto.FileType, OwnerId = "user-id"
            });
        var mapper = new DocumentMapper(service.Object, storage.Object);
        await using var stream = new MemoryStream(new byte[] { 1 });
        var request = new CreateDocumentRequest
        {
            Name = "Document",
            File = new FormFile(stream, 0, stream.Length, "File", "client.pdf")
            { Headers = new HeaderDictionary(), ContentType = "application/pdf" }
        };

        var result = await mapper.CreateAsync(request, "user-id");

        Assert.Equal("generated-guid.pdf", result.FilePath);
        service.Verify(x => x.CreateAsync(
            It.Is<Domain.Dto.Documents.CreateDocumentDto>(dto =>
                dto.FilePath == "generated-guid.pdf" &&
                dto.FileType == "application/pdf"), "user-id"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectEmptyFile()
    {
        var storage = new Mock<IFileStorageService>();
        var mapper = new DocumentMapper(new Mock<IDocumentService>().Object, storage.Object);
        await using var stream = new MemoryStream();
        var request = new CreateDocumentRequest
        {
            File = new FormFile(stream, 0, 0, "File", "empty.pdf")
        };

        await Assert.ThrowsAsync<ArgumentException>(() => mapper.CreateAsync(request, "user-id"));
        storage.Verify(x => x.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteStoredFileAfterDatabaseDelete()
    {
        var document = new DocumentEntity
        {
            Id = Guid.NewGuid(), OwnerId = "user-id", FilePath = "generated-guid.pdf"
        };
        var service = new Mock<IDocumentService>();
        service.Setup(x => x.GetByIdAsync(document.Id, "user-id")).ReturnsAsync(document);
        service.Setup(x => x.DeleteAsync(document.Id, "user-id")).ReturnsAsync(true);
        var storage = new Mock<IFileStorageService>();
        var mapper = new DocumentMapper(service.Object, storage.Object);

        Assert.True(await mapper.DeleteAsync(document.Id, "user-id"));
        storage.Verify(x => x.DeleteAsync("generated-guid.pdf", It.IsAny<CancellationToken>()), Times.Once);
    }
}
