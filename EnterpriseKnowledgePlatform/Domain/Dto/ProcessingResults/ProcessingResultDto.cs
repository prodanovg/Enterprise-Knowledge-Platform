using Domain.Enums;
namespace Domain.Dto.ProcessingResults;
public class ProcessingResultDto { public List<ProcessingSemanticBlockDto> SemanticBlocks { get; set; } = new(); public List<ProcessingTripleDto> Triples { get; set; } = new(); }
public class ProcessingSemanticBlockDto { public string Text { get; set; } = string.Empty; public int BlockIndex { get; set; } public int Page { get; set; } }
public class ProcessingTripleDto { public string Subject { get; set; } = string.Empty; public string Predicate { get; set; } = string.Empty; public string Object { get; set; } = string.Empty; public decimal Confidence { get; set; } public TripleStatus Status { get; set; } public int SemanticBlockIndex { get; set; } }
