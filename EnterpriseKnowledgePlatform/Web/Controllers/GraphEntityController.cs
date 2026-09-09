using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.GraphEntities;
using Web.Response;
using Web.Response.GraphEntities;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GraphEntityController : ControllerBase
{
    private readonly GraphEntityMapper _graphEntityMapper;

    public GraphEntityController(GraphEntityMapper graphEntityMapper)
    {
        _graphEntityMapper = graphEntityMapper;
    }

    [HttpPost]
    public async Task<ActionResult<GraphEntityResponse>> Create(
        CreateGraphEntityRequest request)
    {
        try
        {
            var result = await _graphEntityMapper.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GraphEntityResponse>> GetById(Guid id)
    {
        var result = await _graphEntityMapper.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<GraphEntityResponse>>> GetAll() =>
        Ok(await _graphEntityMapper.GetAllAsync());

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<GraphEntityResponse>>> GetAllPaged(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1) return BadRequest();
        return Ok(await _graphEntityMapper.GetAllPagedAsync(pageNumber, pageSize));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GraphEntityResponse>> Update(
        Guid id, UpdateGraphEntityRequest request)
    {
        try
        {
            return Ok(await _graphEntityMapper.UpdateAsync(id, request));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            return await _graphEntityMapper.DeleteAsync(id)
                ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
