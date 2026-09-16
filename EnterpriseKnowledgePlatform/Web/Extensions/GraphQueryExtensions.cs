using Domain.Models;
using Web.Response.Graph;
namespace Web.Extensions;
public static class GraphQueryExtensions
{
    public static GraphEntityResponse ToGraphQueryResponse(this GraphEntity entity) => entity.ToGraphQueryResponse(entity.EntityType);
    public static GraphEntityResponse ToGraphQueryResponse(this GraphEntity entity, EntityType? type) => new() { Id = entity.Id, Name = entity.Name, CanonicalName = entity.CanonicalName, EntityTypeId = entity.EntityTypeId, EntityTypeName = type?.Name ?? string.Empty };
    public static List<GraphEntityResponse> ToGraphQueryResponse(this IEnumerable<GraphEntity> entities) => entities.Select(x => x.ToGraphQueryResponse()).ToList();
    public static GraphRelationshipResponse ToGraphQueryResponse(this GraphRelationship relationship) => new() { Id = relationship.Id, SourceEntityId = relationship.SourceEntityId, TargetEntityId = relationship.TargetEntityId, Predicate = relationship.Predicate, Confidence = relationship.Confidence };
    public static List<GraphRelationshipResponse> ToGraphQueryResponse(this IEnumerable<GraphRelationship> relationships) => relationships.Select(x => x.ToGraphQueryResponse()).ToList();
}
