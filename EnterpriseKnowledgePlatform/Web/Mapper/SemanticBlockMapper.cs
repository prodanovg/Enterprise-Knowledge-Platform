using Domain.Dto.SemanticBlocks;
using Service.Interface;
using Web.Extensions;
using Web.Request.SemanticBlocks;
using Web.Response;
using Web.Response.SemanticBlocks;

namespace Web.Mapper;

public class SemanticBlockMapper
{
    private readonly ISemanticBlockService _semanticBlockService;

    public SemanticBlockMapper(ISemanticBlockService semanticBlockService)
    {
        _semanticBlockService = semanticBlockService;
    }

    public async Task<SemanticBlockResponse> CreateAsync(
        CreateSemanticBlockRequest request, string userId)
    {
        var semanticBlock = await _semanticBlockService
            .CreateAsync(ToDto(request), userId);
        return semanticBlock.ToResponse();
    }

    public async Task<SemanticBlockResponse?> GetByIdAsync(Guid id, string userId)
    {
        var semanticBlock = await _semanticBlockService.GetByIdAsync(id, userId);
        return semanticBlock?.ToResponse();
    }

    public async Task<List<SemanticBlockResponse>> GetAllAsync(string userId)
    {
        var semanticBlocks = await _semanticBlockService.GetAllAsync(userId);
        return semanticBlocks.ToResponse();
    }

    public async Task<PaginatedResponse<SemanticBlockResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId)
    {
        var semanticBlocks = await _semanticBlockService
            .GetAllPagedAsync(pageNumber, pageSize, userId);
        return semanticBlocks.ToResponse();
    }

    public async Task<SemanticBlockResponse> UpdateAsync(
        Guid id, UpdateSemanticBlockRequest request, string userId)
    {
        var semanticBlock = await _semanticBlockService
            .UpdateAsync(id, ToDto(request), userId);
        return semanticBlock.ToResponse();
    }

    public Task<bool> DeleteAsync(Guid id, string userId)
    {
        return _semanticBlockService.DeleteAsync(id, userId);
    }

    public static CreateSemanticBlockDto ToDto(
        CreateSemanticBlockRequest request)
    {
        return new CreateSemanticBlockDto
        {
            DocumentId = request.DocumentId,
            Text = request.Text,
            BlockIndex = request.BlockIndex,
            Page = request.Page
        };
    }

    public static UpdateSemanticBlockDto ToDto(
        UpdateSemanticBlockRequest request)
    {
        return new UpdateSemanticBlockDto
        {
            Text = request.Text,
            BlockIndex = request.BlockIndex,
            Page = request.Page
        };
    }
}
