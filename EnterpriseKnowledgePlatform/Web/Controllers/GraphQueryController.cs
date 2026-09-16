using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Response.Graph;
namespace Web.Controllers;
[ApiController, Route("api/[controller]"), Authorize]
public class GraphQueryController : ControllerBase
{
    private readonly GraphQueryMapper _mapper;
    public GraphQueryController(GraphQueryMapper mapper) => _mapper = mapper;
    [HttpGet("search")] public Task<List<GraphEntityResponse>> Search([FromQuery] string? query) => _mapper.SearchAsync(query);
    [HttpGet("entity/{id:guid}")] public async Task<ActionResult<EntityGraphResponse>> GetEntity(Guid id) => (await _mapper.GetEntityGraphAsync(id)) is { } result ? Ok(result) : NotFound();
    [HttpGet("subgraph/{id:guid}")] public async Task<ActionResult<GraphSubgraphResponse>> GetSubgraph(Guid id, [FromQuery] int depth = 1) { if (depth < 1 || depth > 2) return BadRequest("Depth must be between 1 and 2."); var result = await _mapper.GetSubgraphAsync(id, depth); return result == null ? NotFound() : Ok(result); }
}
