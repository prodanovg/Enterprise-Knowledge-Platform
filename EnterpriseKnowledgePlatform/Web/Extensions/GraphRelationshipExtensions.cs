using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.GraphRelationships;

namespace Web.Extensions;

public static class GraphRelationshipExtensions
{
    public static GraphRelationshipResponse ToResponse(this GraphRelationship relationship) => new()
    {
        Id = relationship.Id,
        SourceEntityId = relationship.SourceEntityId,
        TargetEntityId = relationship.TargetEntityId,
        Predicate = relationship.Predicate,
        Confidence = relationship.Confidence,
        CreatedAt = relationship.CreatedAt
    };

    public static List<GraphRelationshipResponse> ToResponse(
        this IEnumerable<GraphRelationship> relationships) =>
        relationships.Select(x => x.ToResponse()).ToList();

    public static PaginatedResponse<GraphRelationshipResponse> ToResponse(
        this PaginatedResult<GraphRelationship> result) => new()
    {
        Items = result.Items.ToResponse(),
        TotalCount = result.TotalCount,
        PageNumber = result.PageNumber,
        PageSize = result.PageSize,
        TotalPages = result.TotalPages
    };
}
