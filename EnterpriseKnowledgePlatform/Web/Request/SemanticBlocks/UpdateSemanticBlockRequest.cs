namespace Web.Request.SemanticBlocks;

public class UpdateSemanticBlockRequest
{
    public string Text { get; set; } = string.Empty;
    public int BlockIndex { get; set; }
    public int Page { get; set; }
}
