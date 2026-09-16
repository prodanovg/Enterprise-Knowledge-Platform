using Service.Interface;
using Web.Extensions;
using Web.Response.Graph;
namespace Web.Mapper;
public class GraphQueryMapper
{
    private readonly IGraphQueryService _service;
    public GraphQueryMapper(IGraphQueryService service) => _service = service;
    public async Task<List<GraphEntityResponse>> SearchAsync(string? query) => (await _service.SearchAsync(query)).ToGraphQueryResponse();
    public async Task<EntityGraphResponse?> GetEntityGraphAsync(Guid id)
    { var result = await _service.GetEntityGraphAsync(id); return result == null ? null : new EntityGraphResponse { Entity = result.Entity.ToGraphQueryResponse(result.EntityType), OutgoingRelationships = result.OutgoingRelationships.ToGraphQueryResponse(), IncomingRelationships = result.IncomingRelationships.ToGraphQueryResponse(), ConnectedEntities = result.ConnectedEntities.ToGraphQueryResponse() }; }
    public async Task<GraphSubgraphResponse?> GetSubgraphAsync(Guid id, int depth) { var result = await _service.GetSubgraphAsync(id, depth); return result == null ? null : new GraphSubgraphResponse { Entities = result.Entities.ToGraphQueryResponse(), Relationships = result.Relationships.ToGraphQueryResponse() }; }
}
