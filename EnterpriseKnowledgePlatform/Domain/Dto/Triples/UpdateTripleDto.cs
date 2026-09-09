using Domain.Enums;

namespace Domain.Dto.Triples;

public class UpdateTripleDto
{
    public string Subject { get; set; } = string.Empty;
    public string Predicate { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public TripleStatus Status { get; set; }
}
