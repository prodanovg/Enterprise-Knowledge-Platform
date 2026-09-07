using Domain.Dto;
using Domain.Dto.Documents;
using Domain.Enums;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class DocumentService : IDocumentService
{
    private readonly IRepository<Document> _documentRepository;

    public DocumentService(IRepository<Document> documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<DocumentDto> CreateAsync(
        CreateDocumentDto dto,
        string userId)
    {
        var document = new Document
        {
            Name = dto.Name,
            FilePath = dto.FilePath,
            FileType = dto.FileType,
            Status = DocumentStatus.Pending,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = userId
        };

        await _documentRepository.InsertAsync(document);
        await _documentRepository.SaveChangesAsync();
        
        return MapToDto(document);
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid id)
    {
        return await _documentRepository.GetAsync(
            selector: x => MapToDto(x),
            predicate: x => x.Id == id,
            asNoTracking: true);
    }

    public async Task<List<DocumentDto>> GetAllAsync()
    {
        return await _documentRepository.GetAllAsync(
            selector: x => MapToDto(x));
    }

    public async Task<PaginatedResult<DocumentDto>> GetAllPagedAsync(
        int pageNumber,
        int pageSize)
    {
        return await _documentRepository.GetAllPagedAsync(
            selector: x => MapToDto(x),
            pageNumber: pageNumber,
            pageSize: pageSize,
            asNoTracking: true);
    }

    public async Task<DocumentDto> UpdateAsync(
        Guid id,
        UpdateDocumentDto dto)
    {
        var document = await _documentRepository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id);

        if (document == null)
        {
            throw new KeyNotFoundException(
                $"Document with ID '{id}' was not found.");
        }

        document.Name = dto.Name;
        document.Status = dto.Status;
        document.ModifiedAt = DateTime.UtcNow;

        await _documentRepository.UpdateAsync(document);
        await _documentRepository.SaveChangesAsync();

        return MapToDto(document);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var document = await _documentRepository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id);

        if (document == null)
        {
            return false;
        }

        await _documentRepository.DeleteAsync(document);
        await _documentRepository.SaveChangesAsync();

        return true;
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            Name = document.Name,
            FilePath = document.FilePath,
            FileType = document.FileType,
            CreatedAt = document.CreatedAt,
            Status = document.Status
        };
    }
}