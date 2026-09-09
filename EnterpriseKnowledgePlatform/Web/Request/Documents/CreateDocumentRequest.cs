namespace Web.Request.Documents;

public class CreateDocumentRequest
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}
