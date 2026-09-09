using Domain.Dto.Authentication;
using Service.Interface;
using Web.Extensions;
using Web.Request.Authentication;
using Web.Response.Authentication;

namespace Web.Mapper;

public class AuthMapper
{
    private readonly IAuthService _authService;

    public AuthMapper(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(ToDto(request));
        return result.ToResponse();
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var result = await _authService.LoginAsync(ToDto(request));
        return result.ToResponse();
    }

    public static RegisterDto ToDto(RegisterRequest request)
    {
        return new RegisterDto
        {
            Name = request.Name,
            Username = request.Username,
            Email = request.Email,
            Password = request.Password
        };
    }

    public static LoginDto ToDto(LoginRequest request)
    {
        return new LoginDto
        {
            Username = request.Username,
            Password = request.Password
        };
    }
}
