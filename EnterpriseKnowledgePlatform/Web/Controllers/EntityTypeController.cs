using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.EntityTypes;
using Web.Response;
using Web.Response.EntityTypes;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntityTypeController : ControllerBase
{
    private readonly EntityTypeMapper _entityTypeMapper;

    public EntityTypeController(EntityTypeMapper entityTypeMapper)
    {
        _entityTypeMapper = entityTypeMapper;
    }

    [HttpPost]
    public async Task<ActionResult<EntityTypeResponse>> Create(
        CreateEntityTypeRequest request)
    {
        var result = await _entityTypeMapper.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EntityTypeResponse>> GetById(Guid id)
    {
        var result = await _entityTypeMapper.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<EntityTypeResponse>>> GetAll() =>
        Ok(await _entityTypeMapper.GetAllAsync());

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<EntityTypeResponse>>> GetAllPaged(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1) return BadRequest();
        return Ok(await _entityTypeMapper.GetAllPagedAsync(pageNumber, pageSize));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EntityTypeResponse>> Update(
        Guid id, UpdateEntityTypeRequest request)
    {
        try
        {
            return Ok(await _entityTypeMapper.UpdateAsync(id, request));
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
            return await _entityTypeMapper.DeleteAsync(id)
                ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
