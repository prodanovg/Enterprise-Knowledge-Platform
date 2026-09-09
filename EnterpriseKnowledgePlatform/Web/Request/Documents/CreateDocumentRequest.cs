namespace Web.Request.Documents;

using Microsoft.AspNetCore.Http;

public class CreateDocumentRequest
{
    public string Name { get; set; } = string.Empty;
    public IFormFile? File { get; set; }
}
