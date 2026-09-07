using Domain.Enums;

namespace Domain.Dto.Documents;

public class UpdateDocumentDto
{
    public string Name { get; set; } = string.Empty;
    
    public string FilePath { get; set; } = string.Empty;
    
    public string FileType { get; set; } = string.Empty;
    
    public DocumentStatus Status { get; set; }
}