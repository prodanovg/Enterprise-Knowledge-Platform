using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.GraphRelationships;
using Web.Response;
using Web.Response.GraphRelationships;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GraphRelationshipController : ControllerBase
{
    private readonly GraphRelationshipMapper _mapper;

    public GraphRelationshipController(GraphRelationshipMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<GraphRelationshipResponse>> Create(
        CreateGraphRelationshipRequest request)
    {
        try
        {
            var result = await _mapper.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GraphRelationshipResponse>> GetById(Guid id)
    {
        var result = await _mapper.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<GraphRelationshipResponse>>> GetAll() =>
        Ok(await _mapper.GetAllAsync());

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<GraphRelationshipResponse>>> GetAllPaged(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1) return BadRequest();
        return Ok(await _mapper.GetAllPagedAsync(pageNumber, pageSize));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GraphRelationshipResponse>> Update(
        Guid id, UpdateGraphRelationshipRequest request)
    {
        try
        {
            return Ok(await _mapper.UpdateAsync(id, request));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _mapper.DeleteAsync(id) ? NoContent() : NotFound();
}
