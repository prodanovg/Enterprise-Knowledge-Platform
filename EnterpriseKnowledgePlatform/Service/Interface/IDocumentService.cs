using Domain.Dto;
using Domain.Dto.Documents;

namespace Service.Interface;

public interface IDocumentService
{
    Task<DocumentDto> CreateAsync(CreateDocumentDto dto, string userId);

    Task<DocumentDto?> GetByIdAsync(Guid id);

    Task<List<DocumentDto>> GetAllAsync();

    Task<PaginatedResult<DocumentDto>> GetAllPagedAsync(
        int pageNumber,
        int pageSize);

    Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto dto);

    Task<bool> DeleteAsync(Guid id);
}