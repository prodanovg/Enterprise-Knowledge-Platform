using Domain.Common;

namespace Domain.Models;

public class EntityType : BaseAuditableEntity<string>
{
    public string Name { get; set; } = string.Empty;
}
