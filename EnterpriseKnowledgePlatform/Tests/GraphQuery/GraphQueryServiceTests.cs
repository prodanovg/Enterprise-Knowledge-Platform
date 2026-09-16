using System.Linq.Expressions;
using Moq;
using Repository.Interface;
using Service.Implementation;
using GraphEntityEntity = Domain.Models.GraphEntity;
using EntityTypeEntity = Domain.Models.EntityType;
using RelationshipEntity = Domain.Models.GraphRelationship;

namespace Tests.GraphQueryTests;

public class GraphQueryServiceTests
{
    [Fact]
    public async Task SearchAsync_ReturnsMatchingEntities()
    {
        var repository = new Mock<IRepository<GraphEntityEntity>>();
        var expected = new List<GraphEntityEntity> { new() { Name = "Alice" } };
        repository.Setup(x => x.GetAllAsync<GraphEntityEntity>(It.IsAny<Expression<Func<GraphEntityEntity, GraphEntityEntity>>>(), It.IsAny<Expression<Func<GraphEntityEntity, bool>>>(), It.IsAny<Func<IQueryable<GraphEntityEntity>, IOrderedQueryable<GraphEntityEntity>>>(), null, 50)).ReturnsAsync(expected);
        var service = new GraphQueryService(repository.Object, new Mock<IRepository<EntityTypeEntity>>().Object, new Mock<IRepository<RelationshipEntity>>().Object);
        Assert.Same(expected, await service.SearchAsync("alice"));
    }

    [Fact]
    public async Task GetEntityGraphAsync_ReturnsOutgoingAndIncomingRelationships()
    {
        var id = Guid.NewGuid(); var other = Guid.NewGuid();
        var entities = new Mock<IRepository<GraphEntityEntity>>();
        entities.Setup(x => x.GetAsync<GraphEntityEntity>(It.IsAny<Expression<Func<GraphEntityEntity, GraphEntityEntity>>>(), It.IsAny<Expression<Func<GraphEntityEntity, bool>>>(), null, null, false)).ReturnsAsync(new GraphEntityEntity { Id = id, EntityTypeId = Guid.NewGuid() });
        entities.Setup(x => x.GetAllAsync<GraphEntityEntity>(It.IsAny<Expression<Func<GraphEntityEntity, GraphEntityEntity>>>(), It.IsAny<Expression<Func<GraphEntityEntity, bool>>>(), null, null, null)).ReturnsAsync(new List<GraphEntityEntity> { new() { Id = other } });
        var relationships = new Mock<IRepository<RelationshipEntity>>();
        relationships.SetupSequence(x => x.GetAllAsync<RelationshipEntity>(It.IsAny<Expression<Func<RelationshipEntity, RelationshipEntity>>>(), It.IsAny<Expression<Func<RelationshipEntity, bool>>>(), null, null, null)).ReturnsAsync(new List<RelationshipEntity> { new() { Id = Guid.NewGuid(), SourceEntityId = id, TargetEntityId = other } }).ReturnsAsync(new List<RelationshipEntity> { new() { Id = Guid.NewGuid(), SourceEntityId = other, TargetEntityId = id } });
        var service = new GraphQueryService(entities.Object, new Mock<IRepository<EntityTypeEntity>>().Object, relationships.Object);
        var result = await service.GetEntityGraphAsync(id);
        Assert.Single(result!.OutgoingRelationships); Assert.Single(result.IncomingRelationships);
    }

    [Fact]
    public async Task GetSubgraphAsync_RejectsDepthAboveTwo()
    {
        var service = new GraphQueryService(new Mock<IRepository<GraphEntityEntity>>().Object, new Mock<IRepository<EntityTypeEntity>>().Object, new Mock<IRepository<RelationshipEntity>>().Object);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetSubgraphAsync(Guid.NewGuid(), 3));
    }
}
