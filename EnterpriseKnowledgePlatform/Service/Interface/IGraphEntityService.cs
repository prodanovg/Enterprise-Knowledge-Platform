using Domain.Dto;
using Domain.Dto.GraphEntities;
using Domain.Models;

namespace Service.Interface;

public interface IGraphEntityService
{
    Task<GraphEntity> CreateAsync(CreateGraphEntityDto dto);
    Task<GraphEntity?> GetByIdAsync(Guid id);
    Task<List<GraphEntity>> GetAllAsync();
    Task<PaginatedResult<GraphEntity>> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<GraphEntity> UpdateAsync(Guid id, UpdateGraphEntityDto dto);
    Task<bool> DeleteAsync(Guid id);
}
