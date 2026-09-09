using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.TripleProvenance;
using Web.Response;
using Web.Response.TripleProvenance;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripleProvenanceController : ControllerBase
{
    private readonly TripleProvenanceMapper _tripleProvenanceMapper;

    public TripleProvenanceController(
        TripleProvenanceMapper tripleProvenanceMapper)
    {
        _tripleProvenanceMapper = tripleProvenanceMapper;
    }

    [HttpPost]
    public async Task<ActionResult<TripleProvenanceResponse>> Create(
        [FromBody] CreateTripleProvenanceRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var provenance = await _tripleProvenanceMapper
                .CreateAsync(request, userId);
            return CreatedAtAction(
                nameof(GetById), new { id = provenance.Id }, provenance);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TripleProvenanceResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var provenance = await _tripleProvenanceMapper.GetByIdAsync(id, userId);
        if (provenance == null)
        {
            return NotFound();
        }

        return Ok(provenance);
    }

    [HttpGet]
    public async Task<ActionResult<List<TripleProvenanceResponse>>> GetAll()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _tripleProvenanceMapper.GetAllAsync(userId));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<TripleProvenanceResponse>>>
        GetAllPaged(
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

        return Ok(await _tripleProvenanceMapper.GetAllPagedAsync(
            pageNumber, pageSize, userId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TripleProvenanceResponse>> Update(
        Guid id, [FromBody] UpdateTripleProvenanceRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _tripleProvenanceMapper
                .UpdateAsync(id, request, userId));
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

        if (!await _tripleProvenanceMapper.DeleteAsync(id, userId))
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
