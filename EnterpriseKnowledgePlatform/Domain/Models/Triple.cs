using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class Triple : BaseAuditableEntity<string>
{
    public string Subject { get; set; } = string.Empty;
    public string Predicate { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public TripleStatus Status { get; set; }
}
