using Domain.Dto.Documents;
using Service.Interface;
using Web.Extensions;
using Web.Request.Documents;
using Web.Response;
using Web.Response.Documents;

namespace Web.Mapper;

public class DocumentMapper
{
    private readonly IDocumentService _documentService;

    public DocumentMapper(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public async Task<DocumentResponse> CreateAsync(
        CreateDocumentRequest request, string userId)
    {
        var document = await _documentService.CreateAsync(ToDto(request), userId);
        return document.ToResponse();
    }

    public async Task<DocumentResponse?> GetByIdAsync(Guid id, string userId)
    {
        var document = await _documentService.GetByIdAsync(id, userId);
        return document?.ToResponse();
    }

    public async Task<List<DocumentResponse>> GetAllAsync(string userId)
    {
        var documents = await _documentService.GetAllAsync(userId);
        return documents.ToResponse();
    }

    public async Task<PaginatedResponse<DocumentResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId)
    {
        var documents = await _documentService
            .GetAllPagedAsync(pageNumber, pageSize, userId);
        return documents.ToResponse();
    }

    public async Task<DocumentResponse> UpdateAsync(
        Guid id, UpdateDocumentRequest request, string userId)
    {
        var document = await _documentService
            .UpdateAsync(id, ToDto(request), userId);
        return document.ToResponse();
    }

    public Task<bool> DeleteAsync(Guid id, string userId)
    {
        return _documentService.DeleteAsync(id, userId);
    }

    public static CreateDocumentDto ToDto(CreateDocumentRequest request)
    {
        return new CreateDocumentDto
        {
            Name = request.Name,
            FilePath = request.FilePath,
            FileType = request.FileType
        };
    }

    public static UpdateDocumentDto ToDto(UpdateDocumentRequest request)
    {
        return new UpdateDocumentDto
        {
            Name = request.Name,
            Status = request.Status
        };
    }
}
