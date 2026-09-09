using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.SemanticBlocks;
using Web.Response;
using Web.Response.SemanticBlocks;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SemanticBlockController : ControllerBase
{
    private readonly SemanticBlockMapper _semanticBlockMapper;

    public SemanticBlockController(SemanticBlockMapper semanticBlockMapper)
    {
        _semanticBlockMapper = semanticBlockMapper;
    }

    [HttpPost]
    public async Task<ActionResult<SemanticBlockResponse>> Create(
        [FromBody] CreateSemanticBlockRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var semanticBlock = await _semanticBlockMapper.CreateAsync(request, userId);
            return CreatedAtAction(
                nameof(GetById), new { id = semanticBlock.Id }, semanticBlock);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SemanticBlockResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var semanticBlock = await _semanticBlockMapper.GetByIdAsync(id, userId);
        if (semanticBlock == null)
        {
            return NotFound();
        }

        return Ok(semanticBlock);
    }

    [HttpGet]
    public async Task<ActionResult<List<SemanticBlockResponse>>> GetAll()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _semanticBlockMapper.GetAllAsync(userId));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<SemanticBlockResponse>>> GetAllPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1)
        {
            return BadRequest("Page number must be greater than 0.");
        }

        if (pageSize < 1)
        {
            return BadRequest("Page size must be greater than 0.");
        }

        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _semanticBlockMapper.GetAllPagedAsync(
            pageNumber, pageSize, userId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SemanticBlockResponse>> Update(
        Guid id, [FromBody] UpdateSemanticBlockRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _semanticBlockMapper.UpdateAsync(id, request, userId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        if (!await _semanticBlockMapper.DeleteAsync(id, userId))
        {
            return NotFound();
        }

        return NoContent();
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
