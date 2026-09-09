using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.ApiKeys;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using ApiKeyEntity = Domain.Models.ApiKey;

namespace Tests.ApiKey;

public class ApiKeyServiceTests
{
    private readonly Mock<IRepository<ApiKeyEntity>> _repository = new();
    private readonly ApiKeyService _service;

    public ApiKeyServiceTests() => _service = new(_repository.Object);

    [Fact]
    public async Task CreateAsync_ShouldGenerateAndHashKeyForUser()
    {
        ApiKeyEntity? persisted = null;
        _repository.Setup(x => x.InsertAsync(It.IsAny<ApiKeyEntity>()))
            .Callback<ApiKeyEntity>(x => persisted = x)
            .ReturnsAsync((ApiKeyEntity x) => x);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.CreateAsync(
            new CreateApiKeyDto { Label = "Integration" }, "user-id");

        Assert.False(string.IsNullOrWhiteSpace(result.PlaintextKey));
        Assert.NotNull(persisted);
        Assert.Equal("user-id", persisted!.UserId);
        Assert.NotEqual(result.PlaintextKey, persisted.KeyHash);
        Assert.DoesNotContain(result.PlaintextKey, persisted.KeyHash);
        Assert.Equal("Integration", result.ApiKey.Label);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldUseOwnershipFilter()
    {
        var key = NewKey("user-id");
        _repository.Setup(x => x.GetAsync<ApiKeyEntity>(
                It.IsAny<Expression<Func<ApiKeyEntity, ApiKeyEntity>>>(),
                It.Is<Expression<Func<ApiKeyEntity, bool>>>(p => p.Compile()(key)),
                null, null, true))
            .ReturnsAsync(key);

        Assert.Same(key, await _service.GetByIdAsync(key.Id, "user-id"));
    }

    [Fact]
    public async Task GetAllAndPagedAsync_ShouldReturnOwnedResults()
    {
        var keys = new List<ApiKeyEntity> { NewKey("user-id") };
        _repository.Setup(x => x.GetAllAsync<ApiKeyEntity>(
                It.IsAny<Expression<Func<ApiKeyEntity, ApiKeyEntity>>>(),
                It.IsAny<Expression<Func<ApiKeyEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ApiKeyEntity>, IOrderedQueryable<ApiKeyEntity>>>(), null, null))
            .ReturnsAsync(keys);
        var page = new PaginatedResult<ApiKeyEntity>
        { Items = keys, TotalCount = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        _repository.Setup(x => x.GetAllPagedAsync<ApiKeyEntity>(
                It.IsAny<Expression<Func<ApiKeyEntity, ApiKeyEntity>>>(), 1, 10,
                It.IsAny<Expression<Func<ApiKeyEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ApiKeyEntity>, IOrderedQueryable<ApiKeyEntity>>>(), null, true))
            .ReturnsAsync(page);

        Assert.Same(keys, await _service.GetAllAsync("user-id"));
        Assert.Same(page, await _service.GetAllPagedAsync(1, 10, "user-id"));
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldRejectInvalidPaging()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetAllPagedAsync(0, 10, "user-id"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetAllPagedAsync(1, 0, "user-id"));
    }

    [Fact]
    public async Task UpdateAsync_ShouldChangeOnlyMetadata()
    {
        var key = NewKey("user-id");
        var hash = key.KeyHash;
        SetupGet(key, false);
        _repository.Setup(x => x.UpdateAsync(key)).ReturnsAsync(key);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.UpdateAsync(key.Id,
            new UpdateApiKeyDto { Label = "Updated", IsActive = false }, "user-id");

        Assert.Equal("Updated", result.Label);
        Assert.False(result.IsActive);
        Assert.Equal(hash, result.KeyHash);
        Assert.Equal("user-id", result.UserId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowWhenMissingOrNotOwned()
    {
        SetupGet(null, false);
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), new UpdateApiKeyDto(), "user-id"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrueOrFalse()
    {
        var key = NewKey("user-id");
        SetupGet(key, false);
        _repository.Setup(x => x.DeleteAsync(key)).ReturnsAsync(key);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        Assert.True(await _service.DeleteAsync(key.Id, "user-id"));

        SetupGet(null, false);
        Assert.False(await _service.DeleteAsync(Guid.NewGuid(), "user-id"));
    }

    private void SetupGet(ApiKeyEntity? value, bool noTracking)
    {
        _repository.Setup(x => x.GetAsync<ApiKeyEntity>(
                It.IsAny<Expression<Func<ApiKeyEntity, ApiKeyEntity>>>(),
                It.IsAny<Expression<Func<ApiKeyEntity, bool>>>(), null, null, noTracking))
            .ReturnsAsync(value);
    }

    private static ApiKeyEntity NewKey(string userId) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, KeyHash = "hash", Label = "Key"
    };
}
