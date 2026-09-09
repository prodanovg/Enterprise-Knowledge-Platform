using Domain.Enums;

namespace Web.Request.Triples;

public class CreateTripleRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Predicate { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public TripleStatus Status { get; set; }
}
