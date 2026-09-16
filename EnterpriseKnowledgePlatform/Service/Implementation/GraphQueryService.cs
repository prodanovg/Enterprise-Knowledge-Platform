using Domain.Dto.GraphQuery;
using Domain.Models;
using Repository.Interface;
using Service.Interface;
namespace Service.Implementation;
public class GraphQueryService : IGraphQueryService
{
    private const int SearchLimit = 50;
    private readonly IRepository<GraphEntity> _entities;
    private readonly IRepository<EntityType> _types;
    private readonly IRepository<GraphRelationship> _relationships;
    public GraphQueryService(IRepository<GraphEntity> entities, IRepository<EntityType> types, IRepository<GraphRelationship> relationships)
    { _entities = entities; _types = types; _relationships = relationships; }
    public Task<List<GraphEntity>> SearchAsync(string? query) => _entities.GetAllAsync<GraphEntity>(x => x,
        string.IsNullOrWhiteSpace(query) ? null : x => x.Name.ToLower().Contains(query!.ToLower()) || x.CanonicalName.ToLower().Contains(query.ToLower()),
        x => x.OrderBy(entity => entity.Name), take: SearchLimit);
    public async Task<GraphEntityGraphResult?> GetEntityGraphAsync(Guid id)
    {
        var entity = await _entities.GetAsync<GraphEntity>(x => x, x => x.Id == id);
        if (entity == null) return null;
        var type = await _types.GetAsync<EntityType>(x => x, x => x.Id == entity.EntityTypeId);
        var outgoing = await _relationships.GetAllAsync<GraphRelationship>(x => x, x => x.SourceEntityId == id);
        var incoming = await _relationships.GetAllAsync<GraphRelationship>(x => x, x => x.TargetEntityId == id);
        var ids = outgoing.Select(x => x.TargetEntityId).Concat(incoming.Select(x => x.SourceEntityId)).Where(x => x != id).Distinct().ToList();
        var connected = ids.Count == 0 ? new List<GraphEntity>() : await _entities.GetAllAsync<GraphEntity>(x => x, x => ids.Contains(x.Id));
        return new GraphEntityGraphResult { Entity = entity, EntityType = type ?? new EntityType { Id = entity.EntityTypeId }, OutgoingRelationships = outgoing, IncomingRelationships = incoming, ConnectedEntities = connected };
    }
    public async Task<GraphSubgraphResult?> GetSubgraphAsync(Guid id, int depth = 1)
    {
        if (depth < 1 || depth > 2) throw new ArgumentOutOfRangeException(nameof(depth));
        if (await _entities.GetAsync<GraphEntity>(x => x, x => x.Id == id) == null) return null;
        var entityIds = new HashSet<Guid> { id }; var relationshipIds = new HashSet<Guid>(); var frontier = new HashSet<Guid> { id };
        for (var level = 0; level < depth; level++)
        {
            var relationships = await _relationships.GetAllAsync<GraphRelationship>(x => x, x => frontier.Contains(x.SourceEntityId) || frontier.Contains(x.TargetEntityId));
            var next = new HashSet<Guid>();
            foreach (var relationship in relationships)
            {
                relationshipIds.Add(relationship.Id);
                var other = frontier.Contains(relationship.SourceEntityId) ? relationship.TargetEntityId : relationship.SourceEntityId;
                if (entityIds.Add(other)) next.Add(other);
            }
            frontier = next; if (frontier.Count == 0) break;
        }
        var entities = await _entities.GetAllAsync<GraphEntity>(x => x, x => entityIds.Contains(x.Id));
        var allRelationships = await _relationships.GetAllAsync<GraphRelationship>(x => x, x => relationshipIds.Contains(x.Id));
        return new GraphSubgraphResult { Entities = entities, Relationships = allRelationships };
    }
}
