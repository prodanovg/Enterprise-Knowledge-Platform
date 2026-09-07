using System.Security.Claims;
using Domain.Dto;
using Domain.Dto.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost]
    public async Task<ActionResult<DocumentDto>> Create(
        [FromBody] CreateDocumentDto dto)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var document = await _documentService.CreateAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var document = await _documentService.GetByIdAsync(id, userId);

        if (document == null)
        {
            return NotFound();
        }

        return Ok(document);
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> GetAll()
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var documents = await _documentService.GetAllAsync(userId);
        return Ok(documents);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResult<DocumentDto>>>
        GetAllPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
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

        var result = await _documentService.GetAllPagedAsync(pageNumber, pageSize, userId);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> Update(Guid id, [FromBody] UpdateDocumentDto dto)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var document = await _documentService.UpdateAsync(id, dto, userId);

            return Ok(document);
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

        var deleted = await _documentService.DeleteAsync(id, userId);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier);
    }
}