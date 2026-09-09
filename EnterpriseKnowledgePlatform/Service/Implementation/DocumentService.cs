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

    public async Task<Document> CreateAsync(CreateDocumentDto dto, string userId)
    {
        var now = DateTime.UtcNow;
        var document = new Document
        {
            Name = dto.Name,
            FilePath = dto.FilePath,
            FileType = dto.FileType,
            Status = DocumentStatus.Pending,
            OwnerId = userId,
            CreatedAt = now,
            CreatedBy = userId,
            ModifiedAt = now,
            ModifiedBy = userId
        };

        await _documentRepository.InsertAsync(document);
        await _documentRepository.SaveChangesAsync();

        return document;
    }

    public Task<Document?> GetByIdAsync(Guid id, string userId)
    {
        return _documentRepository.GetAsync<Document>(
            selector: x => x,
            predicate: x => x.Id == id && x.OwnerId == userId,
            asNoTracking: true);
    }

    public Task<List<Document>> GetAllAsync(string userId)
    {
        return _documentRepository.GetAllAsync<Document>(
            selector: x => x,
            predicate: x => x.OwnerId == userId,
            orderBy: x => x.OrderByDescending(d => d.CreatedAt));
    }

    public async Task<PaginatedResult<Document>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException(
                "Page number must be greater than or equal to 1.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentException(
                "Page size must be greater than zero.");
        }

        return await _documentRepository.GetAllPagedAsync<Document>(
            selector: x => x,
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: x => x.OwnerId == userId,
            orderBy: x => x.OrderByDescending(d => d.CreatedAt),
            asNoTracking: true);
    }

    public async Task<Document> UpdateAsync(
        Guid id, UpdateDocumentDto dto, string userId)
    {
        var document = await _documentRepository.GetAsync<Document>(
            selector: x => x,
            predicate: x => x.Id == id && x.OwnerId == userId);

        if (document == null)
        {
            throw new KeyNotFoundException(
                $"Document with ID '{id}' was not found.");
        }

        document.Name = dto.Name;
        document.Status = dto.Status;
        document.ModifiedAt = DateTime.UtcNow;
        document.ModifiedBy = userId;

        await _documentRepository.UpdateAsync(document);
        await _documentRepository.SaveChangesAsync();

        return document;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var document = await _documentRepository.GetAsync<Document>(
            selector: x => x,
            predicate: x => x.Id == id && x.OwnerId == userId);

        if (document == null)
        {
            return false;
        }

        await _documentRepository.DeleteAsync(document);
        await _documentRepository.SaveChangesAsync();

        return true;
    }
}
