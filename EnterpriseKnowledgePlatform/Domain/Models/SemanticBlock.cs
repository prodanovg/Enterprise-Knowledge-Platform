using Domain.Common;

namespace Domain.Models;

public class SemanticBlock : BaseAuditableEntity<string>
{
    public Guid DocumentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int BlockIndex { get; set; }
    public int Page { get; set; }
}
