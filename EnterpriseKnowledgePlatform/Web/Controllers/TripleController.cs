using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.Triples;
using Web.Response;
using Web.Response.Triples;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripleController : ControllerBase
{
    private readonly TripleMapper _tripleMapper;

    public TripleController(TripleMapper tripleMapper)
    {
        _tripleMapper = tripleMapper;
    }

    [HttpPost]
    public async Task<ActionResult<TripleResponse>> Create(CreateTripleRequest request)
    {
        var triple = await _tripleMapper.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = triple.Id }, triple);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TripleResponse>> GetById(Guid id)
    {
        var triple = await _tripleMapper.GetByIdAsync(id);
        return triple == null ? NotFound() : Ok(triple);
    }

    [HttpGet]
    public async Task<ActionResult<List<TripleResponse>>> GetAll()
    {
        return Ok(await _tripleMapper.GetAllAsync());
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<TripleResponse>>> GetAllPaged(
        int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest();
        }

        return Ok(await _tripleMapper.GetAllPagedAsync(pageNumber, pageSize));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TripleResponse>> Update(
        Guid id, UpdateTripleRequest request)
    {
        try
        {
            return Ok(await _tripleMapper.UpdateAsync(id, request));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _tripleMapper.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
