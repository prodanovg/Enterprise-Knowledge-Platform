using Domain.Dto;
using Domain.Dto.Documents;

namespace Service.Interface;

public interface IDocumentService
{
    Task<DocumentDto> CreateAsync(CreateDocumentDto dto, string userId);

    Task<DocumentDto?> GetByIdAsync(Guid id, string userId);

    Task<List<DocumentDto>> GetAllAsync(string userId);

    Task<PaginatedResult<DocumentDto>> GetAllPagedAsync(int pageNumber, int pageSize, string userId);

    Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto dto, string userId);

    Task<bool> DeleteAsync(Guid id, string userId);
}