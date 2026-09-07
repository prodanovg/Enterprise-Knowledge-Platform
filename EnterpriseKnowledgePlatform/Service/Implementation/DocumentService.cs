using System.Linq.Expressions;
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

    private static readonly Expression<Func<Document, DocumentDto>>
        DocumentSelector = x => new DocumentDto
        {
            Id = x.Id,
            Name = x.Name,
            FilePath = x.FilePath,
            FileType = x.FileType,
            CreatedAt = x.CreatedAt,
            Status = x.Status
        };

    public DocumentService(
        IRepository<Document> documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<DocumentDto> CreateAsync(CreateDocumentDto dto, string userId)
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

        return MapToDto(document);
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid id, string userId)
    {
        return await _documentRepository.GetAsync<DocumentDto>(
            selector: DocumentSelector,
            predicate: x =>
                x.Id == id &&
                x.OwnerId == userId,
            asNoTracking: true);
    }

    public async Task<List<DocumentDto>> GetAllAsync(string userId)
    {
        return await _documentRepository.GetAllAsync<DocumentDto>(
            selector: DocumentSelector,
            predicate: x => x.OwnerId == userId,
            orderBy: x => x.OrderByDescending(d => d.CreatedAt));
    }

    public async Task<PaginatedResult<DocumentDto>> GetAllPagedAsync(int pageNumber, int pageSize, string userId)
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

        return await _documentRepository.GetAllPagedAsync<DocumentDto>(
            selector: DocumentSelector,
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: x => x.OwnerId == userId,
            orderBy: x => x.OrderByDescending(d => d.CreatedAt),
            asNoTracking: true);
    }

    public async Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto dto, string userId)
    {
        var document = await _documentRepository.GetAsync<Document>(
            selector: x => x,
            predicate: x =>
                x.Id == id &&
                x.OwnerId == userId);

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

        return MapToDto(document);
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var document = await _documentRepository.GetAsync<Document>(
            selector: x => x,
            predicate: x =>
                x.Id == id &&
                x.OwnerId == userId);

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