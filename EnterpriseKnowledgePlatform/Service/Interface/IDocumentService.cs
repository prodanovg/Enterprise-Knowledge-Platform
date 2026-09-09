using Domain.Dto;
using Domain.Dto.Documents;
using Domain.Models;

namespace Service.Interface;

public interface IDocumentService
{
    Task<Document> CreateAsync(CreateDocumentDto dto, string userId);
    Task<Document?> GetByIdAsync(Guid id, string userId);
    Task<List<Document>> GetAllAsync(string userId);
    Task<PaginatedResult<Document>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId);
    Task<Document> UpdateAsync(Guid id, UpdateDocumentDto dto, string userId);
    Task<bool> DeleteAsync(Guid id, string userId);
}

