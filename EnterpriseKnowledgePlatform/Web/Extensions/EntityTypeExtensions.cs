using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.EntityTypes;

namespace Web.Extensions;

public static class EntityTypeExtensions
{
    public static EntityTypeResponse ToResponse(this EntityType entityType) => new()
    {
        Id = entityType.Id,
        Name = entityType.Name,
        CreatedAt = entityType.CreatedAt
    };

    public static List<EntityTypeResponse> ToResponse(
        this IEnumerable<EntityType> entityTypes) =>
        entityTypes.Select(x => x.ToResponse()).ToList();

    public static PaginatedResponse<EntityTypeResponse> ToResponse(
        this PaginatedResult<EntityType> result) => new()
    {
        Items = result.Items.ToResponse(),
        TotalCount = result.TotalCount,
        PageNumber = result.PageNumber,
        PageSize = result.PageSize,
        TotalPages = result.TotalPages
    };
}
