using Domain.Enums;

namespace Web.Response.Triples;

public class TripleResponse
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Predicate { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public TripleStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
