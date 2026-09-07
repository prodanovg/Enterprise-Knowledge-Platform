using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class User : BaseAuditableEntity<string>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
