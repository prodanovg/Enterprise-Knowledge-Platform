using System.ComponentModel.DataAnnotations;

namespace Web.Request.Users;

public class UpdateUserRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
