using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.Notifications;
using Web.Response;
using Web.Response.Notifications;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly NotificationMapper _notificationMapper;

    public NotificationController(NotificationMapper notificationMapper)
    {
        _notificationMapper = notificationMapper;
    }

    [HttpPost]
    public async Task<ActionResult<NotificationResponse>> Create(
        CreateNotificationRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _notificationMapper.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _notificationMapper.GetByIdAsync(id, userId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationResponse>>> GetAll()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        return Ok(await _notificationMapper.GetAllAsync(userId));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<NotificationResponse>>> GetAllPaged(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1) return BadRequest();
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        return Ok(await _notificationMapper.GetAllPagedAsync(pageNumber, pageSize, userId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NotificationResponse>> Update(
        Guid id, UpdateNotificationRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        try
        {
            return Ok(await _notificationMapper.UpdateAsync(id, request, userId));
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
        return await _notificationMapper.DeleteAsync(id, userId)
            ? NoContent() : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
}
