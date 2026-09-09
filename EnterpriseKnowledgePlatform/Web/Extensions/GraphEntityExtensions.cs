using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.GraphEntities;

namespace Web.Extensions;

public static class GraphEntityExtensions
{
    public static GraphEntityResponse ToResponse(this GraphEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        CanonicalName = entity.CanonicalName,
        EntityTypeId = entity.EntityTypeId,
        CreatedAt = entity.CreatedAt
    };

    public static List<GraphEntityResponse> ToResponse(
        this IEnumerable<GraphEntity> entities) =>
        entities.Select(x => x.ToResponse()).ToList();

    public static PaginatedResponse<GraphEntityResponse> ToResponse(
        this PaginatedResult<GraphEntity> result) => new()
    {
        Items = result.Items.ToResponse(),
        TotalCount = result.TotalCount,
        PageNumber = result.PageNumber,
        PageSize = result.PageSize,
        TotalPages = result.TotalPages
    };
}
