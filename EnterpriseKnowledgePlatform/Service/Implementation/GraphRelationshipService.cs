using Domain.Dto;
using Domain.Dto.GraphRelationships;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class GraphRelationshipService : IGraphRelationshipService
{
    private readonly IRepository<GraphRelationship> _relationshipRepository;
    private readonly IRepository<GraphEntity> _entityRepository;

    public GraphRelationshipService(
        IRepository<GraphRelationship> relationshipRepository,
        IRepository<GraphEntity> entityRepository)
    {
        _relationshipRepository = relationshipRepository;
        _entityRepository = entityRepository;
    }

    public async Task<GraphRelationship> CreateAsync(CreateGraphRelationshipDto dto)
    {
        await ValidateEntitiesAsync(dto.SourceEntityId, dto.TargetEntityId);
        var now = DateTime.UtcNow;
        var relationship = new GraphRelationship
        {
            SourceEntityId = dto.SourceEntityId,
            TargetEntityId = dto.TargetEntityId,
            Predicate = dto.Predicate,
            Confidence = dto.Confidence,
            CreatedAt = now,
            ModifiedAt = now
        };
        await _relationshipRepository.InsertAsync(relationship);
        await _relationshipRepository.SaveChangesAsync();
        return relationship;
    }

    public Task<GraphRelationship?> GetByIdAsync(Guid id) =>
        _relationshipRepository.GetAsync<GraphRelationship>(x => x,
            x => x.Id == id, asNoTracking: true);

    public Task<List<GraphRelationship>> GetAllAsync() =>
        _relationshipRepository.GetAllAsync<GraphRelationship>(x => x,
            orderBy: x => x.OrderByDescending(relationship => relationship.CreatedAt));

    public async Task<PaginatedResult<GraphRelationship>> GetAllPagedAsync(
        int pageNumber, int pageSize)
    {
        ValidatePaging(pageNumber, pageSize);
        return await _relationshipRepository.GetAllPagedAsync<GraphRelationship>(
            x => x, pageNumber, pageSize,
            orderBy: x => x.OrderByDescending(relationship => relationship.CreatedAt),
            asNoTracking: true);
    }

    public async Task<GraphRelationship> UpdateAsync(
        Guid id, UpdateGraphRelationshipDto dto)
    {
        var relationship = await _relationshipRepository.GetAsync<GraphRelationship>(
            x => x, x => x.Id == id);
        if (relationship == null)
        {
            throw new KeyNotFoundException(
                $"Graph relationship with ID '{id}' was not found.");
        }

        await ValidateEntitiesAsync(dto.SourceEntityId, dto.TargetEntityId);
        relationship.SourceEntityId = dto.SourceEntityId;
        relationship.TargetEntityId = dto.TargetEntityId;
        relationship.Predicate = dto.Predicate;
        relationship.Confidence = dto.Confidence;
        relationship.ModifiedAt = DateTime.UtcNow;
        await _relationshipRepository.UpdateAsync(relationship);
        await _relationshipRepository.SaveChangesAsync();
        return relationship;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var relationship = await _relationshipRepository.GetAsync<GraphRelationship>(
            x => x, x => x.Id == id);
        if (relationship == null) return false;

        await _relationshipRepository.DeleteAsync(relationship);
        await _relationshipRepository.SaveChangesAsync();
        return true;
    }

    private async Task ValidateEntitiesAsync(Guid sourceId, Guid targetId)
    {
        if (!await _entityRepository.ExistsAsync(x => x.Id == sourceId))
        {
            throw new KeyNotFoundException($"Source graph entity with ID '{sourceId}' was not found.");
        }

        if (!await _entityRepository.ExistsAsync(x => x.Id == targetId))
        {
            throw new KeyNotFoundException($"Target graph entity with ID '{targetId}' was not found.");
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
