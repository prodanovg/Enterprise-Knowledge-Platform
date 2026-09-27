using Domain.Dto.Users;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository;
using Web.Request.Users;
using Web.Response.Users;

namespace Web.Mapper;

public class UserMapper
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;

    public UserMapper(UserManager<User> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public virtual async Task<List<UserResponse>> GetAllAsync()
    {
        var users = _userManager.Users.OrderBy(x => x.UserName).ToList();
        var result = new List<UserResponse>();
        foreach (var user in users) result.Add(await ToResponseAsync(user));
        return result;
    }

    public virtual async Task<UserResponse?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user == null ? null : await ToResponseAsync(user);
    }

    public virtual async Task<UserResponse> UpdateAsync(string id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id)
                   ?? throw new KeyNotFoundException();
        var email = request.Email.Trim();
        var duplicate = await _userManager.FindByEmailAsync(email);
        if (duplicate != null && duplicate.Id != user.Id)
            throw new InvalidOperationException("A user with this email already exists.");

        user.Name = request.Name.Trim();
        user.Email = email;
        user.NormalizedEmail = _userManager.NormalizeEmail(email);
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) throw new ArgumentException(string.Join(", ", result.Errors.Select(x => x.Description)));
        return await ToResponseAsync(user);
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;
        var hasDependents = await _db.Documents.AnyAsync(x => x.OwnerId == id)
                            || await _db.ApiKeys.AnyAsync(x => x.UserId == id)
                            || await _db.Notifications.AnyAsync(x => x.UserId == id);
        if (hasDependents)
            throw new InvalidOperationException("The user cannot be deleted while dependent records exist.");
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Description)));
        return true;
    }

    private async Task<UserResponse> ToResponseAsync(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Username = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? user.Role.ToString()
    };
}
