using System.ComponentModel.DataAnnotations;

namespace Domain.Dto.Users;

public class UpdateUserDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
