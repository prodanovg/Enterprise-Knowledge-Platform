using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.SemanticBlocks;
using Domain.Models;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using DocumentEntity = Domain.Models.Document;
using SemanticBlockEntity = Domain.Models.SemanticBlock;

namespace Tests.SemanticBlock;

public class SemanticBlockServiceTests
{
    private readonly Mock<IRepository<SemanticBlockEntity>> _semanticBlockRepositoryMock;
    private readonly Mock<IRepository<DocumentEntity>> _documentRepositoryMock;
    private readonly SemanticBlockService _semanticBlockService;

    public SemanticBlockServiceTests()
    {
        _semanticBlockRepositoryMock = new Mock<IRepository<SemanticBlockEntity>>();
        _documentRepositoryMock = new Mock<IRepository<DocumentEntity>>();
        _semanticBlockService = new SemanticBlockService(
            _semanticBlockRepositoryMock.Object,
            _documentRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateBlock_WhenDocumentBelongsToUser()
    {
        var userId = "user-id";
        var document = new DocumentEntity { Id = Guid.NewGuid(), OwnerId = userId };
        var dto = new CreateSemanticBlockDto
        {
            DocumentId = document.Id,
            Text = "Block text",
            BlockIndex = 2,
            Page = 3
        };
        _documentRepositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(document);
        _semanticBlockRepositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<SemanticBlockEntity>()))
            .ReturnsAsync((SemanticBlockEntity block) => block);
        _semanticBlockRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _semanticBlockService.CreateAsync(dto, userId);

        Assert.Equal(dto.DocumentId, result.DocumentId);
        Assert.Equal(dto.Text, result.Text);
        Assert.Equal(dto.BlockIndex, result.BlockIndex);
        Assert.Equal(dto.Page, result.Page);
        Assert.Equal(userId, result.CreatedBy);
        _semanticBlockRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDocumentDoesNotExist()
    {
        _documentRepositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync((DocumentEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _semanticBlockService.CreateAsync(
                new CreateSemanticBlockDto { DocumentId = Guid.NewGuid() },
                "user-id"));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDocumentBelongsToAnotherUser()
    {
        var document = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            OwnerId = "another-user"
        };
        _documentRepositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.Is<Expression<Func<DocumentEntity, bool>>>(predicate =>
                    !predicate.Compile()(document)), null, null, false))
            .ReturnsAsync((DocumentEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _semanticBlockService.CreateAsync(
                new CreateSemanticBlockDto { DocumentId = document.Id },
                "user-id"));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenBlockIsOwned()
    {
        var block = CreateBlock("user-id");
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                It.IsAny<Expression<Func<SemanticBlockEntity, bool>>>(),
                null, null, true))
            .ReturnsAsync(block);

        var result = await _semanticBlockService.GetByIdAsync(block.Id, "user-id");

        Assert.Same(block, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBlockIsNotOwned()
    {
        var block = CreateBlock("another-user");
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                It.Is<Expression<Func<SemanticBlockEntity, bool>>>(predicate =>
                    !predicate.Compile()(block)), null, null, true))
            .ReturnsAsync((SemanticBlockEntity?)null);

        var result = await _semanticBlockService.GetByIdAsync(block.Id, "user-id");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        var blocks = new List<SemanticBlockEntity> { CreateBlock("user-id") };
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAllAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                It.IsAny<Expression<Func<SemanticBlockEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SemanticBlockEntity>, IOrderedQueryable<SemanticBlockEntity>>>(),
                null, null))
            .ReturnsAsync(blocks);

        var result = await _semanticBlockService.GetAllAsync("user-id");

        Assert.Same(blocks, result);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldReturnEntitiesAndUsePagination()
    {
        var page = new PaginatedResult<SemanticBlockEntity>
        {
            Items = new List<SemanticBlockEntity> { CreateBlock("user-id") },
            TotalCount = 1,
            PageNumber = 2,
            PageSize = 5,
            TotalPages = 1
        };
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAllPagedAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                2, 5,
                It.IsAny<Expression<Func<SemanticBlockEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SemanticBlockEntity>, IOrderedQueryable<SemanticBlockEntity>>>(),
                null, true))
            .ReturnsAsync(page);

        var result = await _semanticBlockService.GetAllPagedAsync(2, 5, "user-id");

        Assert.Same(page, result);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldThrow_WhenArgumentsAreInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _semanticBlockService.GetAllPagedAsync(0, 10, "user-id"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _semanticBlockService.GetAllPagedAsync(1, 0, "user-id"));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedEntity()
    {
        var userId = "user-id";
        var block = CreateBlock(userId);
        var dto = new UpdateSemanticBlockDto
        {
            Text = "Updated",
            BlockIndex = 4,
            Page = 5
        };
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                It.IsAny<Expression<Func<SemanticBlockEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(block);
        _semanticBlockRepositoryMock.Setup(x => x.UpdateAsync(block)).ReturnsAsync(block);
        _semanticBlockRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _semanticBlockService.UpdateAsync(block.Id, dto, userId);

        Assert.Same(block, result);
        Assert.Equal(dto.Text, result.Text);
        Assert.Equal(userId, result.ModifiedBy);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenBlockIsNotOwned()
    {
        var block = CreateBlock("another-user");
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                It.Is<Expression<Func<SemanticBlockEntity, bool>>>(predicate =>
                    !predicate.Compile()(block)), null, null, false))
            .ReturnsAsync((SemanticBlockEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _semanticBlockService.UpdateAsync(
                block.Id, new UpdateSemanticBlockDto(), "user-id"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenBlockIsOwned()
    {
        var block = CreateBlock("user-id");
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                It.IsAny<Expression<Func<SemanticBlockEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(block);
        _semanticBlockRepositoryMock.Setup(x => x.DeleteAsync(block)).ReturnsAsync(block);
        _semanticBlockRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _semanticBlockService.DeleteAsync(block.Id, "user-id");

        Assert.True(result);
        _semanticBlockRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenBlockIsNotOwned()
    {
        var block = CreateBlock("another-user");
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                It.Is<Expression<Func<SemanticBlockEntity, bool>>>(predicate =>
                    !predicate.Compile()(block)), null, null, false))
            .ReturnsAsync((SemanticBlockEntity?)null);

        var result = await _semanticBlockService.DeleteAsync(block.Id, "user-id");

        Assert.False(result);
        _semanticBlockRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<SemanticBlockEntity>()), Times.Never);
    }

    private static SemanticBlockEntity CreateBlock(string ownerId)
    {
        var document = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId
        };
        return new SemanticBlockEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Document = document,
            Text = "Block text",
            BlockIndex = 1,
            Page = 1,
            CreatedAt = DateTime.UtcNow
        };
    }
}

