namespace Domain.Dto.SemanticBlocks;

public class UpdateSemanticBlockDto
{
    public string Text { get; set; } = string.Empty;
    public int BlockIndex { get; set; }
    public int Page { get; set; }
}
