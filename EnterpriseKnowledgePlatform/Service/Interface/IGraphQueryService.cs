using Domain.Dto.GraphQuery;
using Domain.Models;
namespace Service.Interface;
public interface IGraphQueryService
{
    Task<List<GraphEntity>> SearchAsync(string? query);
    Task<GraphEntityGraphResult?> GetEntityGraphAsync(Guid id);
    Task<GraphSubgraphResult?> GetSubgraphAsync(Guid id, int depth = 1);
}
