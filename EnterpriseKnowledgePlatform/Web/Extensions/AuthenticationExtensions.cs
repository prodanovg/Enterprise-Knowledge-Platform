using Domain.Dto.Authentication;
using Web.Response.Authentication;

namespace Web.Extensions;

public static class AuthenticationExtensions
{
    public static AuthResponse ToResponse(this AuthResponseDto result)
    {
        return new AuthResponse
        {
            Token = result.Token,
            Expiration = result.Expiration,
            Username = result.Username,
            Email = result.Email,
            Role = result.Role
        };
    }
}
