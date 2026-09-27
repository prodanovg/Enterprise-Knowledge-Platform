using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request.Users;
using Web.Response.Users;

namespace Web.Controllers;

[ApiController, Route("api/[controller]"), Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly UserMapper _mapper;
    public UserController(UserMapper mapper) => _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll() => Ok(await _mapper.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(string id)
    {
        var user = await _mapper.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> Update(string id, [FromBody] UpdateUserRequest request)
    {
        try { return Ok(await _mapper.UpdateAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try { return await _mapper.DeleteAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
