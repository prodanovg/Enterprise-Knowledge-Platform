namespace Web.Response.SemanticBlocks;

public class SemanticBlockResponse
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int BlockIndex { get; set; }
    public int Page { get; set; }
    public DateTime CreatedAt { get; set; }
}
