using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.Documents;
using Web.Response;
using Web.Response.Documents;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly DocumentMapper _documentMapper;

    public DocumentController(DocumentMapper documentMapper)
    {
        _documentMapper = documentMapper;
    }

    [HttpPost]
    public async Task<ActionResult<DocumentResponse>> Create(
        [FromForm] CreateDocumentRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var document = await _documentMapper.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
        }
        catch (ArgumentException)
        {
            return BadRequest("An uploaded file is required and must not be empty.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var document = await _documentMapper.GetByIdAsync(id, userId);
        if (document == null)
        {
            return NotFound();
        }

        return Ok(document);
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentResponse>>> GetAll()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _documentMapper.GetAllAsync(userId));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<DocumentResponse>>> GetAllPaged(
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

        return Ok(await _documentMapper.GetAllPagedAsync(
            pageNumber, pageSize, userId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> Update(
        Guid id, [FromBody] UpdateDocumentRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _documentMapper.UpdateAsync(id, request, userId));
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

        if (!await _documentMapper.DeleteAsync(id, userId))
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
