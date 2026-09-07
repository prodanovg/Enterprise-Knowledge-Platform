using Domain.Common;

namespace Domain.Models;

public class SemanticBlock : BaseAuditableEntity<string>
{
    public Guid DocumentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int BlockIndex { get; set; }
    public int Page { get; set; }

    public Document Document { get; set; } = null!;

    public ICollection<TripleProvenance> TripleProvenances { get; set; } =
        new List<TripleProvenance>();
}