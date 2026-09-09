using Domain.Dto;
using Domain.Dto.SemanticBlocks;
using Domain.Models;

namespace Service.Interface;

public interface ISemanticBlockService
{
    Task<SemanticBlock> CreateAsync(CreateSemanticBlockDto dto, string userId);
    Task<SemanticBlock?> GetByIdAsync(Guid id, string userId);
    Task<List<SemanticBlock>> GetAllAsync(string userId);
    Task<PaginatedResult<SemanticBlock>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId);
    Task<SemanticBlock> UpdateAsync(
        Guid id, UpdateSemanticBlockDto dto, string userId);
    Task<bool> DeleteAsync(Guid id, string userId);
}

