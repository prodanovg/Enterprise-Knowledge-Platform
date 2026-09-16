using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.ProcessingJobs;
using Web.Response;
using Web.Response.ProcessingJobs;
using Service.Interface;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcessingJobController : ControllerBase
{
    private readonly ProcessingJobMapper _processingJobMapper;
    private readonly ProcessingResultMapper? _processingResultMapper;

    public ProcessingJobController(ProcessingJobMapper processingJobMapper, ProcessingResultMapper? processingResultMapper = null)
    {
        _processingJobMapper = processingJobMapper;
        _processingResultMapper = processingResultMapper;
    }

    [HttpPost("{id:guid}/result")]
    public async Task<ActionResult<ProcessingResultSummary>> Result(Guid id, ProcessingResultRequest request)
    {
        try { return Ok(await _processingResultMapper!.PersistAsync(id, request, HttpContext.RequestAborted)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException) { return Conflict(); }
        catch (ArgumentException) { return BadRequest(); }
    }

    [HttpPost]
    public async Task<ActionResult<ProcessingJobResponse>> Create(
        [FromBody] CreateProcessingJobRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var processingJob = await _processingJobMapper.CreateAsync(request, userId);
            return CreatedAtAction(
                nameof(GetById), new { id = processingJob.Id }, processingJob);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("run")]
    public async Task<ActionResult<List<ProcessingJobResponse>>> StartProcessing(
        [FromBody] StartProcessingRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _processingJobMapper.StartProcessingAsync(request, userId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<ActionResult<ProcessingJobResponse>> Retry(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        try { return Ok(await _processingJobMapper.RetryAsync(id, userId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException) { return Conflict(); }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProcessingJobResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var processingJob = await _processingJobMapper.GetByIdAsync(id, userId);
        if (processingJob == null)
        {
            return NotFound();
        }

        return Ok(processingJob);
    }

    [HttpGet]
    public async Task<ActionResult<List<ProcessingJobResponse>>> GetAll()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _processingJobMapper.GetAllAsync(userId));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResponse<ProcessingJobResponse>>> GetAllPaged(
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

        return Ok(await _processingJobMapper.GetAllPagedAsync(
            pageNumber, pageSize, userId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProcessingJobResponse>> Update(
        Guid id, [FromBody] UpdateProcessingJobRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _processingJobMapper.UpdateAsync(id, request, userId));
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

        if (!await _processingJobMapper.DeleteAsync(id, userId))
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
