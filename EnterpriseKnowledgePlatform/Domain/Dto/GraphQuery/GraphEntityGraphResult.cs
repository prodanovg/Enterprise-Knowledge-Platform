using Domain.Models;
namespace Domain.Dto.GraphQuery;
public class GraphEntityGraphResult
{
    public GraphEntity Entity { get; set; } = null!;
    public EntityType EntityType { get; set; } = null!;
    public List<GraphRelationship> OutgoingRelationships { get; set; } = new();
    public List<GraphRelationship> IncomingRelationships { get; set; } = new();
    public List<GraphEntity> ConnectedEntities { get; set; } = new();
}
