using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.ApiKeys;
using Web.Response;
using Web.Response.ApiKeys;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiKeyController : ControllerBase
{
    private readonly ApiKeyMapper _apiKeyMapper;

    public ApiKeyController(ApiKeyMapper apiKeyMapper)
    {
        _apiKeyMapper = apiKeyMapper;
    }

    [HttpPost]
    public async Task<ActionResult<CreateApiKeyResponse>> Create(
        CreateApiKeyRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _apiKeyMapper.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiKeyResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apiKeyMapper.GetByIdAsync(id, userId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<ApiKeyResponse>>> GetAll()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        return Ok(await _apiKeyMapper.GetAllAsync(userId));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<ApiKeyResponse>>> GetAllPaged(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1) return BadRequest();
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        return Ok(await _apiKeyMapper.GetAllPagedAsync(pageNumber, pageSize, userId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiKeyResponse>> Update(
        Guid id, UpdateApiKeyRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        try
        {
            return Ok(await _apiKeyMapper.UpdateAsync(id, request, userId));
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
        if (userId == null) return Unauthorized();
        return await _apiKeyMapper.DeleteAsync(id, userId)
            ? NoContent()
            : NotFound();
    }

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);
}
