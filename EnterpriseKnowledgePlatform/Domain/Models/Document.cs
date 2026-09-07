using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class Document : BaseAuditableEntity<string>
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public DocumentStatus Status { get; set; }
}
