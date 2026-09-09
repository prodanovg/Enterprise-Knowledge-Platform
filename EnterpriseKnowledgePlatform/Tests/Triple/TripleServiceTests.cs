using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.Triples;
using Domain.Enums;
using Domain.Models;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using TripleEntity = Domain.Models.Triple;

namespace Tests.Triple;

public class TripleServiceTests
{
    private readonly Mock<IRepository<TripleEntity>> _repository = new();
    private readonly TripleService _service;

    public TripleServiceTests()
    {
        _service = new TripleService(_repository.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTriple()
    {
        _repository.Setup(x => x.InsertAsync(It.IsAny<TripleEntity>()))
            .ReturnsAsync((TripleEntity x) => x);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        var dto = new CreateTripleDto
        {
            Subject = "Alice", Predicate = "knows", Object = "Bob",
            Confidence = 0.9m, Status = TripleStatus.Validated
        };

        var result = await _service.CreateAsync(dto);

        Assert.Equal(dto.Subject, result.Subject);
        Assert.Equal(dto.Status, result.Status);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTripleWhenFound()
    {
        var triple = NewTriple();
        SetupGet(triple);

        Assert.Same(triple, await _service.GetByIdAsync(triple.Id));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullWhenMissing()
    {
        SetupGet(null);

        Assert.Null(await _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAndPagedAsync_ShouldReturnRepositoryResults()
    {
        var triples = new List<TripleEntity> { NewTriple() };
        _repository.Setup(x => x.GetAllAsync<TripleEntity>(It.IsAny<Expression<Func<TripleEntity, TripleEntity>>>(), null,
                It.IsAny<Func<IQueryable<TripleEntity>, IOrderedQueryable<TripleEntity>>>(), null, null))
            .ReturnsAsync(triples);
        var page = new PaginatedResult<TripleEntity> { Items = triples, TotalCount = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        _repository.Setup(x => x.GetAllPagedAsync<TripleEntity>(It.IsAny<Expression<Func<TripleEntity, TripleEntity>>>(), 1, 10,
                null, It.IsAny<Func<IQueryable<TripleEntity>, IOrderedQueryable<TripleEntity>>>(), null, true))
            .ReturnsAsync(page);

        Assert.Same(triples, await _service.GetAllAsync());
        Assert.Same(page, await _service.GetAllPagedAsync(1, 10));
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldRejectInvalidPaging()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(0, 10));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAllPagedAsync(1, 0));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTriple()
    {
        var triple = NewTriple();
        SetupGet(triple);
        _repository.Setup(x => x.UpdateAsync(triple)).ReturnsAsync(triple);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.UpdateAsync(triple.Id, new UpdateTripleDto
        {
            Subject = "Updated", Predicate = "is", Object = "valid",
            Confidence = 0.8m, Status = TripleStatus.Rejected
        });

        Assert.Equal("Updated", result.Subject);
        Assert.Equal(TripleStatus.Rejected, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowWhenMissing()
    {
        SetupGet(null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), new UpdateTripleDto()));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrueOrFalse()
    {
        var triple = NewTriple();
        SetupGet(triple);
        _repository.Setup(x => x.DeleteAsync(triple)).ReturnsAsync(triple);
        _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        Assert.True(await _service.DeleteAsync(triple.Id));

        SetupGet(null);
        Assert.False(await _service.DeleteAsync(Guid.NewGuid()));
    }

    private void SetupGet(TripleEntity? value)
    {
        _repository.Setup(x => x.GetAsync<TripleEntity>(It.IsAny<Expression<Func<TripleEntity, TripleEntity>>>(),
                It.IsAny<Expression<Func<TripleEntity, bool>>>(), null, null, It.IsAny<bool>()))
            .ReturnsAsync(value);
    }

    private static TripleEntity NewTriple() => new() { Id = Guid.NewGuid(), Subject = "Subject" };
}
