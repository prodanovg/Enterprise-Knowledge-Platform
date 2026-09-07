using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.Documents;
using Domain.Enums;
using Domain.Models;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;

namespace Tests.Document;

public class DocumentServiceTests
{
    private readonly Mock<IRepository<Domain.Models.Document>>
        _repositoryMock;

    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _repositoryMock =
            new Mock<IRepository<Domain.Models.Document>>();

        _documentService =
            new DocumentService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateDocument()
    {
        var userId = "user-id";

        var dto = new CreateDocumentDto
        {
            Name = "Test Document",
            FilePath = "/files/test.pdf",
            FileType = "application/pdf"
        };

        _repositoryMock
            .Setup(x => x.InsertAsync(
                It.IsAny<Domain.Models.Document>()))
            .ReturnsAsync(
                (Domain.Models.Document document) => document);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _documentService.CreateAsync(
            dto,
            userId);

        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.FilePath, result.FilePath);
        Assert.Equal(dto.FileType, result.FileType);
        Assert.Equal(DocumentStatus.Pending, result.Status);

        _repositoryMock.Verify(
            x => x.InsertAsync(
                It.Is<Domain.Models.Document>(d =>
                    d.Name == dto.Name &&
                    d.FilePath == dto.FilePath &&
                    d.FileType == dto.FileType &&
                    d.OwnerId == userId &&
                    d.Status == DocumentStatus.Pending)),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDocument()
    {
        var documentId = Guid.NewGuid();
        var userId = "user-id";

        var document = new Domain.Models.Document
        {
            Id = documentId,
            OwnerId = userId,
            Name = "Old Name",
            FilePath = "/files/test.pdf",
            FileType = "application/pdf",
            Status = DocumentStatus.Pending
        };

        var dto = new UpdateDocumentDto
        {
            Name = "New Name",
            Status = DocumentStatus.Processed
        };

        _repositoryMock
            .Setup(x => x.GetAsync<Domain.Models.Document>(
                It.IsAny<Expression<Func<
                    Domain.Models.Document,
                    Domain.Models.Document>>>(),
                It.IsAny<Expression<Func<
                    Domain.Models.Document,
                    bool>>>(),
                null,
                null,
                false))
            .ReturnsAsync(document);

        _repositoryMock
            .Setup(x => x.UpdateAsync(
                It.IsAny<Domain.Models.Document>()))
            .ReturnsAsync(
                (Domain.Models.Document d) => d);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _documentService.UpdateAsync(
            documentId,
            dto,
            userId);

        Assert.Equal("New Name", result.Name);
        Assert.Equal(
            DocumentStatus.Processed,
            result.Status);

        _repositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Domain.Models.Document>(d =>
                    d.Name == "New Name" &&
                    d.Status ==
                    DocumentStatus.Processed &&
                    d.ModifiedBy == userId)),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        var userId = "user-id";

        var dto = new UpdateDocumentDto
        {
            Name = "New Name",
            Status = DocumentStatus.Processed
        };

        _repositoryMock
            .Setup(x => x.GetAsync<Domain.Models.Document>(
                It.IsAny<Expression<Func<
                    Domain.Models.Document,
                    Domain.Models.Document>>>(),
                It.IsAny<Expression<Func<
                    Domain.Models.Document,
                    bool>>>(),
                null,
                null,
                false))
            .ReturnsAsync(
                (Domain.Models.Document?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _documentService.UpdateAsync(
                documentId,
                dto,
                userId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenDocumentExists()
    {
        var documentId = Guid.NewGuid();
        var userId = "user-id";

        var document = new Domain.Models.Document
        {
            Id = documentId,
            OwnerId = userId
        };

        _repositoryMock
            .Setup(x => x.GetAsync<Domain.Models.Document>(
                It.IsAny<Expression<Func<
                    Domain.Models.Document,
                    Domain.Models.Document>>>(),
                It.IsAny<Expression<Func<
                    Domain.Models.Document,
                    bool>>>(),
                null,
                null,
                false))
            .ReturnsAsync(document);

        _repositoryMock
            .Setup(x => x.DeleteAsync(
                It.IsAny<Domain.Models.Document>()))
            .ReturnsAsync(
                (Domain.Models.Document d) => d);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _documentService.DeleteAsync(
            documentId,
            userId);

        Assert.True(result);

        _repositoryMock.Verify(
            x => x.DeleteAsync(document),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        var userId = "user-id";

        _repositoryMock
            .Setup(x => x.GetAsync<Domain.Models.Document>(
                It.IsAny<Expression<Func<
                    Domain.Models.Document,
                    Domain.Models.Document>>>(),
                It.IsAny<Expression<Func<
                    Domain.Models.Document,
                    bool>>>(),
                null,
                null,
                false))
            .ReturnsAsync(
                (Domain.Models.Document?)null);

        var result = await _documentService.DeleteAsync(
            documentId,
            userId);

        Assert.False(result);

        _repositoryMock.Verify(
            x => x.DeleteAsync(
                It.IsAny<Domain.Models.Document>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldThrow_WhenPageNumberIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _documentService.GetAllPagedAsync(
                0,
                10,
                "user-id"));
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldThrow_WhenPageSizeIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _documentService.GetAllPagedAsync(
                1,
                0,
                "user-id"));
    }
}