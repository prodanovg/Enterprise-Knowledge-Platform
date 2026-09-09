using Domain.Dto.GraphEntities;
using Service.Interface;
using Web.Extensions;
using Web.Request.GraphEntities;
using Web.Response;
using Web.Response.GraphEntities;

namespace Web.Mapper;

public class GraphEntityMapper
{
    private readonly IGraphEntityService _graphEntityService;

    public GraphEntityMapper(IGraphEntityService graphEntityService)
    {
        _graphEntityService = graphEntityService;
    }

    public async Task<GraphEntityResponse> CreateAsync(CreateGraphEntityRequest request) =>
        (await _graphEntityService.CreateAsync(ToDto(request))).ToResponse();

    public async Task<GraphEntityResponse?> GetByIdAsync(Guid id) =>
        (await _graphEntityService.GetByIdAsync(id))?.ToResponse();

    public async Task<List<GraphEntityResponse>> GetAllAsync() =>
        (await _graphEntityService.GetAllAsync()).ToResponse();

    public async Task<PaginatedResponse<GraphEntityResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize) =>
        (await _graphEntityService.GetAllPagedAsync(pageNumber, pageSize)).ToResponse();

    public async Task<GraphEntityResponse> UpdateAsync(
        Guid id, UpdateGraphEntityRequest request) =>
        (await _graphEntityService.UpdateAsync(id, ToDto(request))).ToResponse();

    public Task<bool> DeleteAsync(Guid id) => _graphEntityService.DeleteAsync(id);

    public static CreateGraphEntityDto ToDto(CreateGraphEntityRequest request) => new()
    {
        Name = request.Name,
        CanonicalName = request.CanonicalName,
        EntityTypeId = request.EntityTypeId
    };

    public static UpdateGraphEntityDto ToDto(UpdateGraphEntityRequest request) => new()
    {
        Name = request.Name,
        CanonicalName = request.CanonicalName,
        EntityTypeId = request.EntityTypeId
    };
}
