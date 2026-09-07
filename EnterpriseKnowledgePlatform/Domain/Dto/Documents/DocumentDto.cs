using Domain.Enums;

namespace Domain.Dto.Documents;

public class DocumentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DocumentStatus Status { get; set; }
}