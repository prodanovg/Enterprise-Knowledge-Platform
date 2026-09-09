namespace Domain.Dto.SemanticBlocks;

public class CreateSemanticBlockDto
{
    public Guid DocumentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int BlockIndex { get; set; }
    public int Page { get; set; }
}
