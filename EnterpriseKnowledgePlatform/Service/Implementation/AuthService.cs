using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Dto.Authentication;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Interface;

namespace Service.Implementation;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            UserName = dto.Username,
            Email = dto.Email,
            Name = dto.Name
        };

        var result = await _userManager.CreateAsync(
            user,
            dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(x => x.Description));

            throw new InvalidOperationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        var token = await GenerateJwtTokenAsync(user);

        return new AuthResponseDto
        {
            Token = token.Token,
            Expiration = token.Expiration,
            Username = user.UserName!,
            Email = user.Email!,
            Role = "User"
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            dto.Password);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = await GenerateJwtTokenAsync(user);

        return new AuthResponseDto
        {
            Token = token.Token,
            Expiration = token.Expiration,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "User"
        };
    }

    private async Task<(string Token, DateTime Expiration)>
        GenerateJwtTokenAsync(User user)
    {
        var jwtSettings = _configuration
            .GetSection("Jwt");

        var key = jwtSettings["Key"]
                  ?? throw new InvalidOperationException(
                      "JWT Key is not configured.");

        var issuer = jwtSettings["Issuer"]
                     ?? throw new InvalidOperationException(
                         "JWT Issuer is not configured.");

        var audience = jwtSettings["Audience"]
                       ?? throw new InvalidOperationException(
                           "JWT Audience is not configured.");

        var expirationMinutes = int.Parse(
            jwtSettings["ExpirationMinutes"]
            ?? "60");

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(ClaimTypes.Role, role));
        }

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow
            .AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return (tokenString, expiration);
    }
}