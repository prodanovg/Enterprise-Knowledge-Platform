using Domain.Enums;

namespace Web.Response.Documents;

public class DocumentResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
