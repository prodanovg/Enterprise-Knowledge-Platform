using Domain.Dto;
using Domain.Dto.GraphRelationships;
using Domain.Models;

namespace Service.Interface;

public interface IGraphRelationshipService
{
    Task<GraphRelationship> CreateAsync(CreateGraphRelationshipDto dto);
    Task<GraphRelationship?> GetByIdAsync(Guid id);
    Task<List<GraphRelationship>> GetAllAsync();
    Task<PaginatedResult<GraphRelationship>> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<GraphRelationship> UpdateAsync(Guid id, UpdateGraphRelationshipDto dto);
    Task<bool> DeleteAsync(Guid id);
}
