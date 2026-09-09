using Domain.Dto;
using Domain.Dto.EntityTypes;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class EntityTypeService : IEntityTypeService
{
    private readonly IRepository<EntityType> _entityTypeRepository;
    private readonly IRepository<GraphEntity> _graphEntityRepository;

    public EntityTypeService(
        IRepository<EntityType> entityTypeRepository,
        IRepository<GraphEntity> graphEntityRepository)
    {
        _entityTypeRepository = entityTypeRepository;
        _graphEntityRepository = graphEntityRepository;
    }

    public async Task<EntityType> CreateAsync(CreateEntityTypeDto dto)
    {
        var now = DateTime.UtcNow;
        var entityType = new EntityType
        {
            Name = dto.Name,
            CreatedAt = now,
            ModifiedAt = now
        };
        await _entityTypeRepository.InsertAsync(entityType);
        await _entityTypeRepository.SaveChangesAsync();
        return entityType;
    }

    public Task<EntityType?> GetByIdAsync(Guid id) =>
        _entityTypeRepository.GetAsync<EntityType>(x => x,
            x => x.Id == id, asNoTracking: true);

    public Task<List<EntityType>> GetAllAsync() =>
        _entityTypeRepository.GetAllAsync<EntityType>(x => x,
            orderBy: x => x.OrderByDescending(entityType => entityType.CreatedAt));

    public async Task<PaginatedResult<EntityType>> GetAllPagedAsync(
        int pageNumber, int pageSize)
    {
        ValidatePaging(pageNumber, pageSize);
        return await _entityTypeRepository.GetAllPagedAsync<EntityType>(
            x => x, pageNumber, pageSize,
            orderBy: x => x.OrderByDescending(entityType => entityType.CreatedAt),
            asNoTracking: true);
    }

    public async Task<EntityType> UpdateAsync(Guid id, UpdateEntityTypeDto dto)
    {
        var entityType = await _entityTypeRepository.GetAsync<EntityType>(
            x => x, x => x.Id == id);
        if (entityType == null)
        {
            throw new KeyNotFoundException($"Entity type with ID '{id}' was not found.");
        }

        entityType.Name = dto.Name;
        entityType.ModifiedAt = DateTime.UtcNow;
        await _entityTypeRepository.UpdateAsync(entityType);
        await _entityTypeRepository.SaveChangesAsync();
        return entityType;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entityType = await _entityTypeRepository.GetAsync<EntityType>(
            x => x, x => x.Id == id);
        if (entityType == null) return false;

        if (await _graphEntityRepository.ExistsAsync(x => x.EntityTypeId == id))
        {
            throw new InvalidOperationException(
                "The entity type cannot be deleted while graph entities reference it.");
        }

        await _entityTypeRepository.DeleteAsync(entityType);
        await _entityTypeRepository.SaveChangesAsync();
        return true;
    }

    private static void ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than or equal to 1.");
        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.");
    }
}
