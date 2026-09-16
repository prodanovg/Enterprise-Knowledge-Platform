namespace Web.Response.Graph;
public class EntityGraphResponse { public GraphEntityResponse Entity { get; set; } = null!; public List<GraphRelationshipResponse> OutgoingRelationships { get; set; } = new(); public List<GraphRelationshipResponse> IncomingRelationships { get; set; } = new(); public List<GraphEntityResponse> ConnectedEntities { get; set; } = new(); }
