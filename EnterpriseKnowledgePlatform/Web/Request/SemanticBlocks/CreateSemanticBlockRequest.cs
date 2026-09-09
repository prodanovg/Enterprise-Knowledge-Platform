namespace Web.Request.SemanticBlocks;

public class CreateSemanticBlockRequest
{
    public Guid DocumentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int BlockIndex { get; set; }
    public int Page { get; set; }
}
