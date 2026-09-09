using Domain.Enums;

namespace Web.Request.Documents;

public class UpdateDocumentRequest
{
    public string Name { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
}
