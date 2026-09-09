using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.TripleProvenance;
using Domain.Models;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using DocumentEntity = Domain.Models.Document;
using SemanticBlockEntity = Domain.Models.SemanticBlock;
using TripleEntity = Domain.Models.Triple;
using ProvenanceEntity = Domain.Models.TripleProvenance;

namespace Tests.TripleProvenance;

public class TripleProvenanceServiceTests
{
    private readonly Mock<IRepository<ProvenanceEntity>> _provenanceRepositoryMock;
    private readonly Mock<IRepository<TripleEntity>> _tripleRepositoryMock;
    private readonly Mock<IRepository<DocumentEntity>> _documentRepositoryMock;
    private readonly Mock<IRepository<SemanticBlockEntity>> _semanticBlockRepositoryMock;
    private readonly TripleProvenanceService _service;

    public TripleProvenanceServiceTests()
    {
        _provenanceRepositoryMock = new Mock<IRepository<ProvenanceEntity>>();
        _tripleRepositoryMock = new Mock<IRepository<TripleEntity>>();
        _documentRepositoryMock = new Mock<IRepository<DocumentEntity>>();
        _semanticBlockRepositoryMock = new Mock<IRepository<SemanticBlockEntity>>();
        _service = new TripleProvenanceService(
            _provenanceRepositoryMock.Object,
            _tripleRepositoryMock.Object,
            _documentRepositoryMock.Object,
            _semanticBlockRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreate_WhenAllRelationshipsAreValid()
    {
        var userId = "user-id";
        var data = CreateRelationshipData(userId);
        SetupValidRelationships(data);
        _provenanceRepositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<ProvenanceEntity>()))
            .ReturnsAsync((ProvenanceEntity value) => value);
        _provenanceRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.CreateAsync(data.Dto, userId);

        Assert.Equal(data.Dto.TripleId, result.TripleId);
        Assert.Equal(data.Dto.DocumentId, result.DocumentId);
        Assert.Equal(data.Dto.SemanticBlockId, result.SemanticBlockId);
        Assert.Equal(userId, result.CreatedBy);
        _provenanceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDocumentIsMissingOrNotOwned()
    {
        var data = CreateRelationshipData("another-user");
        _documentRepositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync((DocumentEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateAsync(data.Dto, "user-id"));
        _provenanceRepositoryMock.Verify(
            x => x.InsertAsync(It.IsAny<ProvenanceEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTripleIsMissing()
    {
        var data = CreateRelationshipData("user-id");
        SetupDocument(data.Document);
        _tripleRepositoryMock
            .Setup(x => x.GetAsync<TripleEntity>(
                It.IsAny<Expression<Func<TripleEntity, TripleEntity>>>(),
                It.IsAny<Expression<Func<TripleEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync((TripleEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateAsync(data.Dto, "user-id"));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenSemanticBlockDoesNotBelongToDocument()
    {
        var data = CreateRelationshipData("user-id");
        SetupDocument(data.Document);
        SetupTriple(data.Triple);
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                It.IsAny<Expression<Func<SemanticBlockEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync((SemanticBlockEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateAsync(data.Dto, "user-id"));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOwnedProvenance()
    {
        var provenance = CreateProvenance("user-id");
        _provenanceRepositoryMock
            .Setup(x => x.GetAsync<ProvenanceEntity>(
                It.IsAny<Expression<Func<ProvenanceEntity, ProvenanceEntity>>>(),
                It.IsAny<Expression<Func<ProvenanceEntity, bool>>>(),
                null, null, true))
            .ReturnsAsync(provenance);

        var result = await _service.GetByIdAsync(provenance.Id, "user-id");

        Assert.Same(provenance, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldNotReturnAnotherUsersProvenance()
    {
        var provenance = CreateProvenance("another-user");
        _provenanceRepositoryMock
            .Setup(x => x.GetAsync<ProvenanceEntity>(
                It.IsAny<Expression<Func<ProvenanceEntity, ProvenanceEntity>>>(),
                It.Is<Expression<Func<ProvenanceEntity, bool>>>(predicate =>
                    !predicate.Compile()(provenance)), null, null, true))
            .ReturnsAsync((ProvenanceEntity?)null);

        var result = await _service.GetByIdAsync(provenance.Id, "user-id");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnRepositoryResults()
    {
        var results = new List<ProvenanceEntity> { CreateProvenance("user-id") };
        _provenanceRepositoryMock
            .Setup(x => x.GetAllAsync<ProvenanceEntity>(
                It.IsAny<Expression<Func<ProvenanceEntity, ProvenanceEntity>>>(),
                It.IsAny<Expression<Func<ProvenanceEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ProvenanceEntity>, IOrderedQueryable<ProvenanceEntity>>>(),
                null, null))
            .ReturnsAsync(results);

        var result = await _service.GetAllAsync("user-id");

        Assert.Same(results, result);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldReturnRepositoryPage()
    {
        var page = new PaginatedResult<ProvenanceEntity>
        {
            Items = new List<ProvenanceEntity> { CreateProvenance("user-id") },
            TotalCount = 1,
            PageNumber = 2,
            PageSize = 5,
            TotalPages = 1
        };
        _provenanceRepositoryMock
            .Setup(x => x.GetAllPagedAsync<ProvenanceEntity>(
                It.IsAny<Expression<Func<ProvenanceEntity, ProvenanceEntity>>>(),
                2, 5,
                It.IsAny<Expression<Func<ProvenanceEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ProvenanceEntity>, IOrderedQueryable<ProvenanceEntity>>>(),
                null, true))
            .ReturnsAsync(page);

        var result = await _service.GetAllPagedAsync(2, 5, "user-id");

        Assert.Same(page, result);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldThrow_WhenPaginationIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetAllPagedAsync(0, 10, "user-id"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetAllPagedAsync(1, 0, "user-id"));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdate_WhenOwnedAndRelationshipsAreValid()
    {
        var userId = "user-id";
        var existing = CreateProvenance(userId);
        var data = CreateRelationshipData(userId);
        var dto = new UpdateTripleProvenanceDto
        {
            TripleId = data.Dto.TripleId,
            DocumentId = data.Dto.DocumentId,
            SemanticBlockId = data.Dto.SemanticBlockId
        };
        SetupExisting(existing);
        SetupValidRelationships(data);
        _provenanceRepositoryMock.Setup(x => x.UpdateAsync(existing)).ReturnsAsync(existing);
        _provenanceRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.UpdateAsync(existing.Id, dto, userId);

        Assert.Same(existing, result);
        Assert.Equal(dto.TripleId, result.TripleId);
        Assert.Equal(userId, result.ModifiedBy);
        _provenanceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenProvenanceIsNotOwned()
    {
        var existing = CreateProvenance("another-user");
        _provenanceRepositoryMock
            .Setup(x => x.GetAsync<ProvenanceEntity>(
                It.IsAny<Expression<Func<ProvenanceEntity, ProvenanceEntity>>>(),
                It.Is<Expression<Func<ProvenanceEntity, bool>>>(predicate =>
                    !predicate.Compile()(existing)), null, null, false))
            .ReturnsAsync((ProvenanceEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(existing.Id, new UpdateTripleProvenanceDto(), "user-id"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenOwned()
    {
        var provenance = CreateProvenance("user-id");
        SetupExisting(provenance);
        _provenanceRepositoryMock.Setup(x => x.DeleteAsync(provenance)).ReturnsAsync(provenance);
        _provenanceRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.DeleteAsync(provenance.Id, "user-id");

        Assert.True(result);
        _provenanceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotOwnedOrMissing()
    {
        var provenance = CreateProvenance("another-user");
        _provenanceRepositoryMock
            .Setup(x => x.GetAsync<ProvenanceEntity>(
                It.IsAny<Expression<Func<ProvenanceEntity, ProvenanceEntity>>>(),
                It.Is<Expression<Func<ProvenanceEntity, bool>>>(predicate =>
                    !predicate.Compile()(provenance)), null, null, false))
            .ReturnsAsync((ProvenanceEntity?)null);

        var result = await _service.DeleteAsync(provenance.Id, "user-id");

        Assert.False(result);
        _provenanceRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<ProvenanceEntity>()), Times.Never);
    }

    private void SetupValidRelationships(RelationshipData data)
    {
        SetupDocument(data.Document);
        SetupTriple(data.Triple);
        _semanticBlockRepositoryMock
            .Setup(x => x.GetAsync<SemanticBlockEntity>(
                It.IsAny<Expression<Func<SemanticBlockEntity, SemanticBlockEntity>>>(),
                It.IsAny<Expression<Func<SemanticBlockEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(data.SemanticBlock);
    }

    private void SetupDocument(DocumentEntity document)
    {
        _documentRepositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(document);
    }

    private void SetupTriple(TripleEntity triple)
    {
        _tripleRepositoryMock
            .Setup(x => x.GetAsync<TripleEntity>(
                It.IsAny<Expression<Func<TripleEntity, TripleEntity>>>(),
                It.IsAny<Expression<Func<TripleEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(triple);
    }

    private void SetupExisting(ProvenanceEntity provenance)
    {
        _provenanceRepositoryMock
            .Setup(x => x.GetAsync<ProvenanceEntity>(
                It.IsAny<Expression<Func<ProvenanceEntity, ProvenanceEntity>>>(),
                It.IsAny<Expression<Func<ProvenanceEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(provenance);
    }

    private static RelationshipData CreateRelationshipData(string ownerId)
    {
        var document = new DocumentEntity { Id = Guid.NewGuid(), OwnerId = ownerId };
        var semanticBlock = new SemanticBlockEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Document = document
        };
        var triple = new TripleEntity { Id = Guid.NewGuid() };
        return new RelationshipData(
            document,
            semanticBlock,
            triple,
            new CreateTripleProvenanceDto
            {
                TripleId = triple.Id,
                DocumentId = document.Id,
                SemanticBlockId = semanticBlock.Id
            });
    }

    private static ProvenanceEntity CreateProvenance(string ownerId)
    {
        var document = new DocumentEntity { Id = Guid.NewGuid(), OwnerId = ownerId };
        return new ProvenanceEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Document = document,
            TripleId = Guid.NewGuid(),
            SemanticBlockId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private sealed record RelationshipData(
        DocumentEntity Document,
        SemanticBlockEntity SemanticBlock,
        TripleEntity Triple,
        CreateTripleProvenanceDto Dto);
}
