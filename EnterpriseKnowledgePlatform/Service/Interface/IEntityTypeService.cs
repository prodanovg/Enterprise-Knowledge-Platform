using Domain.Dto;
using Domain.Dto.EntityTypes;
using Domain.Models;

namespace Service.Interface;

public interface IEntityTypeService
{
    Task<EntityType> CreateAsync(CreateEntityTypeDto dto);
    Task<EntityType?> GetByIdAsync(Guid id);
    Task<List<EntityType>> GetAllAsync();
    Task<PaginatedResult<EntityType>> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<EntityType> UpdateAsync(Guid id, UpdateEntityTypeDto dto);
    Task<bool> DeleteAsync(Guid id);
}
