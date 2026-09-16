using Domain.Models;
namespace Domain.Dto.GraphQuery;
public class GraphSubgraphResult
{
    public List<GraphEntity> Entities { get; set; } = new();
    public List<GraphRelationship> Relationships { get; set; } = new();
}
