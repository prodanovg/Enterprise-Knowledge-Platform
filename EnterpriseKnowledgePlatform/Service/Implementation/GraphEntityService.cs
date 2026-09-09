using Domain.Dto;
using Domain.Dto.GraphEntities;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class GraphEntityService : IGraphEntityService
{
    private readonly IRepository<GraphEntity> _graphEntityRepository;
    private readonly IRepository<EntityType> _entityTypeRepository;
    private readonly IRepository<GraphRelationship> _relationshipRepository;

    public GraphEntityService(
        IRepository<GraphEntity> graphEntityRepository,
        IRepository<EntityType> entityTypeRepository,
        IRepository<GraphRelationship> relationshipRepository)
    {
        _graphEntityRepository = graphEntityRepository;
        _entityTypeRepository = entityTypeRepository;
        _relationshipRepository = relationshipRepository;
    }

    public async Task<GraphEntity> CreateAsync(CreateGraphEntityDto dto)
    {
        await EnsureEntityTypeExistsAsync(dto.EntityTypeId);
        var now = DateTime.UtcNow;
        var entity = new GraphEntity
        {
            Name = dto.Name,
            CanonicalName = dto.CanonicalName,
            EntityTypeId = dto.EntityTypeId,
            CreatedAt = now,
            ModifiedAt = now
        };
        await _graphEntityRepository.InsertAsync(entity);
        await _graphEntityRepository.SaveChangesAsync();
        return entity;
    }

    public Task<GraphEntity?> GetByIdAsync(Guid id) =>
        _graphEntityRepository.GetAsync<GraphEntity>(x => x,
            x => x.Id == id, asNoTracking: true);

    public Task<List<GraphEntity>> GetAllAsync() =>
        _graphEntityRepository.GetAllAsync<GraphEntity>(x => x,
            orderBy: x => x.OrderByDescending(entity => entity.CreatedAt));

    public async Task<PaginatedResult<GraphEntity>> GetAllPagedAsync(
        int pageNumber, int pageSize)
    {
        ValidatePaging(pageNumber, pageSize);
        return await _graphEntityRepository.GetAllPagedAsync<GraphEntity>(
            x => x, pageNumber, pageSize,
            orderBy: x => x.OrderByDescending(entity => entity.CreatedAt),
            asNoTracking: true);
    }

    public async Task<GraphEntity> UpdateAsync(
        Guid id, UpdateGraphEntityDto dto)
    {
        var entity = await _graphEntityRepository.GetAsync<GraphEntity>(
            x => x, x => x.Id == id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Graph entity with ID '{id}' was not found.");
        }

        await EnsureEntityTypeExistsAsync(dto.EntityTypeId);
        entity.Name = dto.Name;
        entity.CanonicalName = dto.CanonicalName;
        entity.EntityTypeId = dto.EntityTypeId;
        entity.ModifiedAt = DateTime.UtcNow;
        await _graphEntityRepository.UpdateAsync(entity);
        await _graphEntityRepository.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _graphEntityRepository.GetAsync<GraphEntity>(
            x => x, x => x.Id == id);
        if (entity == null) return false;

        var referenced = await _relationshipRepository.ExistsAsync(
            x => x.SourceEntityId == id || x.TargetEntityId == id);
        if (referenced)
        {
            throw new InvalidOperationException(
                "The graph entity cannot be deleted while graph relationships reference it.");
        }

        await _graphEntityRepository.DeleteAsync(entity);
        await _graphEntityRepository.SaveChangesAsync();
        return true;
    }

    private async Task EnsureEntityTypeExistsAsync(Guid id)
    {
        if (!await _entityTypeRepository.ExistsAsync(x => x.Id == id))
        {
            throw new KeyNotFoundException($"Entity type with ID '{id}' was not found.");
        }
    }

    private static void ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than or equal to 1.");
        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.");
    }
}
