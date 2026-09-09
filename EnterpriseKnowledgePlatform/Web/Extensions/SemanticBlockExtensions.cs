using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.SemanticBlocks;

namespace Web.Extensions;

public static class SemanticBlockExtensions
{
    public static SemanticBlockResponse ToResponse(
        this SemanticBlock semanticBlock)
    {
        return new SemanticBlockResponse
        {
            Id = semanticBlock.Id,
            DocumentId = semanticBlock.DocumentId,
            Text = semanticBlock.Text,
            BlockIndex = semanticBlock.BlockIndex,
            Page = semanticBlock.Page,
            CreatedAt = semanticBlock.CreatedAt
        };
    }

    public static List<SemanticBlockResponse> ToResponse(
        this IEnumerable<SemanticBlock> semanticBlocks)
    {
        return semanticBlocks
            .Select(semanticBlock => semanticBlock.ToResponse())
            .ToList();
    }

    public static PaginatedResponse<SemanticBlockResponse> ToResponse(
        this PaginatedResult<SemanticBlock> result)
    {
        return new PaginatedResponse<SemanticBlockResponse>
        {
            Items = result.Items.ToResponse(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }
}
