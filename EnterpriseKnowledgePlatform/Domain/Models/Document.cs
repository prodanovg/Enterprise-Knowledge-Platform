using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class Document : BaseAuditableEntity<string>
{
    public string OwnerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }

    public User Owner { get; set; } = null!;

    public ICollection<ProcessingJob> ProcessingJobs { get; set; } =
        new List<ProcessingJob>();

    public ICollection<SemanticBlock> SemanticBlocks { get; set; } =
        new List<SemanticBlock>();

    public ICollection<TripleProvenance> TripleProvenances { get; set; } =
        new List<TripleProvenance>();
}