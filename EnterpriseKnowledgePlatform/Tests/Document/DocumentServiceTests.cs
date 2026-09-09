using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.Documents;
using Domain.Enums;
using Domain.Models;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using DocumentEntity = Domain.Models.Document;

namespace Tests.DocumentFeature;

public class DocumentServiceTests
{
    private readonly Mock<IRepository<DocumentEntity>> _repositoryMock;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _repositoryMock = new Mock<IRepository<DocumentEntity>>();
        _documentService = new DocumentService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateDocument()
    {
        var userId = "user-id";
        var dto = new CreateDocumentDto
        {
            Name = "Test DocumentEntity",
            FilePath = "/files/test.pdf",
            FileType = "application/pdf"
        };

        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<DocumentEntity>()))
            .ReturnsAsync((DocumentEntity document) => document);
        _repositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _documentService.CreateAsync(dto, userId);

        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.FilePath, result.FilePath);
        Assert.Equal(dto.FileType, result.FileType);
        Assert.Equal(DocumentStatus.Pending, result.Status);
        Assert.Equal(userId, result.OwnerId);
        Assert.Equal(userId, result.CreatedBy);
        Assert.Equal(userId, result.ModifiedBy);
        _repositoryMock.Verify(
            x => x.InsertAsync(It.Is<DocumentEntity>(document =>
                document.Status == DocumentStatus.Pending &&
                document.OwnerId == userId)), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedEntity()
    {
        var documentId = Guid.NewGuid();
        var userId = "user-id";
        var document = new DocumentEntity
        {
            Id = documentId,
            OwnerId = userId,
            Name = "Old Name",
            Status = DocumentStatus.Pending
        };
        var dto = new UpdateDocumentDto
        {
            Name = "New Name",
            Status = DocumentStatus.Processed
        };

        _repositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(document);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<DocumentEntity>()))
            .ReturnsAsync((DocumentEntity value) => value);
        _repositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _documentService.UpdateAsync(documentId, dto, userId);

        Assert.Same(document, result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Status, result.Status);
        Assert.Equal(userId, result.ModifiedBy);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenDocumentDoesNotExist()
    {
        _repositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync((DocumentEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _documentService.UpdateAsync(
                Guid.NewGuid(),
                new UpdateDocumentDto(),
                "user-id"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenDocumentExists()
    {
        var document = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            OwnerId = "user-id"
        };
        _repositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(document);
        _repositoryMock.Setup(x => x.DeleteAsync(document)).ReturnsAsync(document);
        _repositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _documentService.DeleteAsync(document.Id, "user-id");

        Assert.True(result);
        _repositoryMock.Verify(x => x.DeleteAsync(document), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenDocumentDoesNotExist()
    {
        _repositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync((DocumentEntity?)null);

        var result = await _documentService.DeleteAsync(Guid.NewGuid(), "user-id");

        Assert.False(result);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<DocumentEntity>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldThrow_WhenPageNumberIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _documentService.GetAllPagedAsync(0, 10, "user-id"));
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldThrow_WhenPageSizeIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _documentService.GetAllPagedAsync(1, 0, "user-id"));
    }
}


