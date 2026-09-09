using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.EntityTypes;
using Domain.Models;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using EntityTypeEntity = Domain.Models.EntityType;
using GraphEntityEntity = Domain.Models.GraphEntity;

namespace Tests.EntityType;

public class EntityTypeServiceTests
{
    private readonly Mock<IRepository<EntityTypeEntity>> _repository = new();
    private readonly Mock<IRepository<GraphEntityEntity>> _graphRepository = new();
    private readonly EntityTypeService _service;

    public EntityTypeServiceTests() =>
        _service = new(_repository.Object, _graphRepository.Object);

    [Fact]
    public async Task CreateAsync_ShouldCreateEntityType()
    {
        _repository.Setup(x => x.InsertAsync(It.IsAny<EntityTypeEntity>()))
            .ReturnsAsync((EntityTypeEntity x) => x);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.CreateAsync(new CreateEntityTypeDto { Name = "Person" });

        Assert.Equal("Person", result.Name);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAndGetAllPagedAsync_ShouldReturnResults()
    {
        var entityType = NewEntityType();
        SetupGet(entityType);
        Assert.Same(entityType, await _service.GetByIdAsync(entityType.Id));

        var items = new List<EntityTypeEntity> { entityType };
        _repository.Setup(x => x.GetAllAsync<EntityTypeEntity>(
                It.IsAny<Expression<Func<EntityTypeEntity, EntityTypeEntity>>>(), null,
                It.IsAny<Func<IQueryable<EntityTypeEntity>, IOrderedQueryable<EntityTypeEntity>>>(), null, null))
            .ReturnsAsync(items);
        var page = new PaginatedResult<EntityTypeEntity>
        { Items = items, TotalCount = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        _repository.Setup(x => x.GetAllPagedAsync<EntityTypeEntity>(
                It.IsAny<Expression<Func<EntityTypeEntity, EntityTypeEntity>>>(), 1, 10,
                null, It.IsAny<Func<IQueryable<EntityTypeEntity>, IOrderedQueryable<EntityTypeEntity>>>(), null, true))
            .ReturnsAsync(page);

        Assert.Same(items, await _service.GetAllAsync());
        Assert.Same(page, await _service.GetAllPagedAsync(1, 10));
    }

    [Fact]
    public async Task GetByIdAndPagedAsync_ShouldHandleMissingAndInvalidInput()
    {
        SetupGet(null);
        Assert.Null(await _service.GetByIdAsync(Guid.NewGuid()));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(0, 10));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(1, 0));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOrThrowWhenMissing()
    {
        var entityType = NewEntityType();
        SetupGet(entityType);
        _repository.Setup(x => x.UpdateAsync(entityType)).ReturnsAsync(entityType);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        var result = await _service.UpdateAsync(entityType.Id,
            new UpdateEntityTypeDto { Name = "Updated" });
        Assert.Equal("Updated", result.Name);

        SetupGet(null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), new UpdateEntityTypeDto()));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteMissingOrUnreferencedType()
    {
        var entityType = NewEntityType();
        SetupGet(entityType);
        _graphRepository.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<GraphEntityEntity, bool>>>()))
            .ReturnsAsync(false);
        _repository.Setup(x => x.DeleteAsync(entityType)).ReturnsAsync(entityType);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        Assert.True(await _service.DeleteAsync(entityType.Id));

        SetupGet(null);
        Assert.False(await _service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRejectReferencedType()
    {
        var entityType = NewEntityType();
        SetupGet(entityType);
        _graphRepository.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<GraphEntityEntity, bool>>>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteAsync(entityType.Id));
        _repository.Verify(x => x.DeleteAsync(It.IsAny<EntityTypeEntity>()), Times.Never);
    }

    private void SetupGet(EntityTypeEntity? value) =>
        _repository.Setup(x => x.GetAsync<EntityTypeEntity>(
                It.IsAny<Expression<Func<EntityTypeEntity, EntityTypeEntity>>>(),
                It.IsAny<Expression<Func<EntityTypeEntity, bool>>>(), null, null, It.IsAny<bool>()))
            .ReturnsAsync(value);

    private static EntityTypeEntity NewEntityType() =>
        new() { Id = Guid.NewGuid(), Name = "Person" };
}
