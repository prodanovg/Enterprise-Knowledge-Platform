using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.GraphRelationships;
using Domain.Models;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using RelationshipEntity = Domain.Models.GraphRelationship;
using GraphEntityEntity = Domain.Models.GraphEntity;

namespace Tests.GraphRelationship;

public class GraphRelationshipServiceTests
{
    private readonly Mock<IRepository<RelationshipEntity>> _repository = new();
    private readonly Mock<IRepository<GraphEntityEntity>> _entities = new();
    private readonly GraphRelationshipService _service;

    public GraphRelationshipServiceTests() =>
        _service = new(_repository.Object, _entities.Object);

    [Fact]
    public async Task CreateAsync_ShouldValidateBothEntitiesAndCreate()
    {
        _entities.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<GraphEntityEntity, bool>>>()))
            .ReturnsAsync(true);
        _repository.Setup(x => x.InsertAsync(It.IsAny<RelationshipEntity>()))
            .ReturnsAsync((RelationshipEntity x) => x);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        var source = Guid.NewGuid();
        var target = Guid.NewGuid();
        var result = await _service.CreateAsync(new CreateGraphRelationshipDto
        { SourceEntityId = source, TargetEntityId = target, Predicate = "knows", Confidence = .9m });
        Assert.Equal(source, result.SourceEntityId);
        Assert.Equal(target, result.TargetEntityId);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectMissingEntity()
    {
        _entities.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<GraphEntityEntity, bool>>>()))
            .ReturnsAsync(false);
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateAsync(new CreateGraphRelationshipDto()));
        _repository.Verify(x => x.InsertAsync(It.IsAny<RelationshipEntity>()), Times.Never);
    }

    [Fact]
    public async Task GetAndPagedAsync_ShouldReturnResultsAndValidatePaging()
    {
        var relationship = NewRelationship();
        SetupGet(relationship);
        Assert.Same(relationship, await _service.GetByIdAsync(relationship.Id));
        SetupGet(null);
        Assert.Null(await _service.GetByIdAsync(Guid.NewGuid()));
        var items = new List<RelationshipEntity> { relationship };
        _repository.Setup(x => x.GetAllAsync<RelationshipEntity>(It.IsAny<Expression<Func<RelationshipEntity, RelationshipEntity>>>(), null,
                It.IsAny<Func<IQueryable<RelationshipEntity>, IOrderedQueryable<RelationshipEntity>>>(), null, null)).ReturnsAsync(items);
        var page = new PaginatedResult<RelationshipEntity> { Items = items, TotalCount = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        _repository.Setup(x => x.GetAllPagedAsync<RelationshipEntity>(It.IsAny<Expression<Func<RelationshipEntity, RelationshipEntity>>>(), 1, 10, null,
                It.IsAny<Func<IQueryable<RelationshipEntity>, IOrderedQueryable<RelationshipEntity>>>(), null, true)).ReturnsAsync(page);
        Assert.Same(items, await _service.GetAllAsync());
        Assert.Same(page, await _service.GetAllPagedAsync(1, 10));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(0, 10));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(1, 0));
    }

    [Fact]
    public async Task UpdateAsync_ShouldValidateAndHandleMissing()
    {
        var relationship = NewRelationship();
        SetupGet(relationship);
        _entities.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<GraphEntityEntity, bool>>>())).ReturnsAsync(true);
        _repository.Setup(x => x.UpdateAsync(relationship)).ReturnsAsync(relationship);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        var result = await _service.UpdateAsync(relationship.Id, new UpdateGraphRelationshipDto { Predicate = "updated" });
        Assert.Equal("updated", result.Predicate);
        SetupGet(null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), new UpdateGraphRelationshipDto()));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrueOrFalse()
    {
        var relationship = NewRelationship();
        SetupGet(relationship);
        _repository.Setup(x => x.DeleteAsync(relationship)).ReturnsAsync(relationship);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        Assert.True(await _service.DeleteAsync(relationship.Id));
        SetupGet(null);
        Assert.False(await _service.DeleteAsync(Guid.NewGuid()));
    }

    private void SetupGet(RelationshipEntity? value) =>
        _repository.Setup(x => x.GetAsync<RelationshipEntity>(It.IsAny<Expression<Func<RelationshipEntity, RelationshipEntity>>>(),
                It.IsAny<Expression<Func<RelationshipEntity, bool>>>(), null, null, It.IsAny<bool>())).ReturnsAsync(value);

    private static RelationshipEntity NewRelationship() => new()
    { Id = Guid.NewGuid(), SourceEntityId = Guid.NewGuid(), TargetEntityId = Guid.NewGuid() };
}
