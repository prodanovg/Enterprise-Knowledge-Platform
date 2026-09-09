using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.GraphEntities;
using Domain.Models;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using GraphEntityEntity = Domain.Models.GraphEntity;
using EntityTypeEntity = Domain.Models.EntityType;
using GraphRelationshipEntity = Domain.Models.GraphRelationship;

namespace Tests.GraphEntity;

public class GraphEntityServiceTests
{
    private readonly Mock<IRepository<GraphEntityEntity>> _repository = new();
    private readonly Mock<IRepository<EntityTypeEntity>> _entityTypes = new();
    private readonly Mock<IRepository<GraphRelationshipEntity>> _relationships = new();
    private readonly GraphEntityService _service;

    public GraphEntityServiceTests() =>
        _service = new(_repository.Object, _entityTypes.Object, _relationships.Object);

    [Fact]
    public async Task CreateAsync_ShouldValidateTypeAndCreate()
    {
        var typeId = Guid.NewGuid();
        _entityTypes.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<EntityTypeEntity, bool>>>()))
            .ReturnsAsync(true);
        _repository.Setup(x => x.InsertAsync(It.IsAny<GraphEntityEntity>()))
            .ReturnsAsync((GraphEntityEntity x) => x);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.CreateAsync(new CreateGraphEntityDto
        { Name = "Alice", CanonicalName = "alice", EntityTypeId = typeId });

        Assert.Equal("Alice", result.Name);
        Assert.Equal(typeId, result.EntityTypeId);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectMissingType()
    {
        _entityTypes.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<EntityTypeEntity, bool>>>()))
            .ReturnsAsync(false);
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateAsync(new CreateGraphEntityDto { EntityTypeId = Guid.NewGuid() }));
        _repository.Verify(x => x.InsertAsync(It.IsAny<GraphEntityEntity>()), Times.Never);
    }

    [Fact]
    public async Task GetAndPagedAsync_ShouldReturnResultsAndValidatePaging()
    {
        var entity = NewEntity();
        SetupGet(entity);
        Assert.Same(entity, await _service.GetByIdAsync(entity.Id));
        SetupGet(null);
        Assert.Null(await _service.GetByIdAsync(Guid.NewGuid()));

        var items = new List<GraphEntityEntity> { entity };
        _repository.Setup(x => x.GetAllAsync<GraphEntityEntity>(It.IsAny<Expression<Func<GraphEntityEntity, GraphEntityEntity>>>(), null,
                It.IsAny<Func<IQueryable<GraphEntityEntity>, IOrderedQueryable<GraphEntityEntity>>>(), null, null))
            .ReturnsAsync(items);
        var page = new PaginatedResult<GraphEntityEntity>
        { Items = items, TotalCount = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        _repository.Setup(x => x.GetAllPagedAsync<GraphEntityEntity>(It.IsAny<Expression<Func<GraphEntityEntity, GraphEntityEntity>>>(), 1, 10,
                null, It.IsAny<Func<IQueryable<GraphEntityEntity>, IOrderedQueryable<GraphEntityEntity>>>(), null, true))
            .ReturnsAsync(page);
        Assert.Same(items, await _service.GetAllAsync());
        Assert.Same(page, await _service.GetAllPagedAsync(1, 10));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(0, 10));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(1, 0));
    }

    [Fact]
    public async Task UpdateAsync_ShouldValidateTypeAndHandleMissing()
    {
        var entity = NewEntity();
        SetupGet(entity);
        _entityTypes.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<EntityTypeEntity, bool>>>()))
            .ReturnsAsync(true);
        _repository.Setup(x => x.UpdateAsync(entity)).ReturnsAsync(entity);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        var result = await _service.UpdateAsync(entity.Id,
            new UpdateGraphEntityDto { Name = "Updated", EntityTypeId = Guid.NewGuid() });
        Assert.Equal("Updated", result.Name);

        SetupGet(null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), new UpdateGraphEntityDto()));
    }

    [Fact]
    public async Task DeleteAsync_ShouldHandleMissingSuccessAndRelationships()
    {
        var entity = NewEntity();
        SetupGet(entity);
        _relationships.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<GraphRelationshipEntity, bool>>>()))
            .ReturnsAsync(false);
        _repository.Setup(x => x.DeleteAsync(entity)).ReturnsAsync(entity);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        Assert.True(await _service.DeleteAsync(entity.Id));
        SetupGet(null);
        Assert.False(await _service.DeleteAsync(Guid.NewGuid()));

        SetupGet(entity);
        _relationships.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<GraphRelationshipEntity, bool>>>()))
            .ReturnsAsync(true);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(entity.Id));
    }

    private void SetupGet(GraphEntityEntity? value) =>
        _repository.Setup(x => x.GetAsync<GraphEntityEntity>(It.IsAny<Expression<Func<GraphEntityEntity, GraphEntityEntity>>>(),
                It.IsAny<Expression<Func<GraphEntityEntity, bool>>>(), null, null, It.IsAny<bool>()))
            .ReturnsAsync(value);

    private static GraphEntityEntity NewEntity() => new()
    { Id = Guid.NewGuid(), Name = "Alice", CanonicalName = "alice", EntityTypeId = Guid.NewGuid() };
}
