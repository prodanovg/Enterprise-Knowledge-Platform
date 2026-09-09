using Domain.Dto.Documents;
using Service.Interface;
using Web.Extensions;
using Web.Request.Documents;
using Web.Response;
using Web.Response.Documents;
using Web.Services;

namespace Web.Mapper;

public class DocumentMapper
{
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorageService;

    public DocumentMapper(
        IDocumentService documentService,
        IFileStorageService fileStorageService)
    {
        _documentService = documentService;
        _fileStorageService = fileStorageService;
    }

    public async Task<DocumentResponse> CreateAsync(
        CreateDocumentRequest request, string userId)
    {
        if (request.File == null || request.File.Length <= 0)
        {
            throw new ArgumentException("Uploaded file must not be empty.");
        }

        var fileReference = await _fileStorageService.SaveAsync(request.File);
        try
        {
            var document = await _documentService.CreateAsync(
                ToDto(request, fileReference), userId);
            return document.ToResponse();
        }
        catch
        {
            await _fileStorageService.DeleteAsync(fileReference);
            throw;
        }
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

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var document = await _documentService.GetByIdAsync(id, userId);
        if (document == null)
        {
            return false;
        }

        var deleted = await _documentService.DeleteAsync(id, userId);
        if (deleted)
        {
            await _fileStorageService.DeleteAsync(document.FilePath);
        }

        return deleted;
    }

    public static CreateDocumentDto ToDto(
        CreateDocumentRequest request, string fileReference)
    {
        return new CreateDocumentDto
        {
            Name = request.Name,
            FilePath = fileReference,
            FileType = !string.IsNullOrWhiteSpace(request.File!.ContentType)
                ? request.File.ContentType
                : Path.GetExtension(request.File.FileName)
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
