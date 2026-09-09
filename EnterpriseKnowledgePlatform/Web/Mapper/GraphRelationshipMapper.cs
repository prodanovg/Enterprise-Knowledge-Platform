using Domain.Dto.GraphRelationships;
using Service.Interface;
using Web.Extensions;
using Web.Request.GraphRelationships;
using Web.Response;
using Web.Response.GraphRelationships;

namespace Web.Mapper;

public class GraphRelationshipMapper
{
    private readonly IGraphRelationshipService _service;

    public GraphRelationshipMapper(IGraphRelationshipService service)
    {
        _service = service;
    }

    public async Task<GraphRelationshipResponse> CreateAsync(CreateGraphRelationshipRequest request) =>
        (await _service.CreateAsync(ToDto(request))).ToResponse();

    public async Task<GraphRelationshipResponse?> GetByIdAsync(Guid id) =>
        (await _service.GetByIdAsync(id))?.ToResponse();

    public async Task<List<GraphRelationshipResponse>> GetAllAsync() =>
        (await _service.GetAllAsync()).ToResponse();

    public async Task<PaginatedResponse<GraphRelationshipResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize) =>
        (await _service.GetAllPagedAsync(pageNumber, pageSize)).ToResponse();

    public async Task<GraphRelationshipResponse> UpdateAsync(
        Guid id, UpdateGraphRelationshipRequest request) =>
        (await _service.UpdateAsync(id, ToDto(request))).ToResponse();

    public Task<bool> DeleteAsync(Guid id) => _service.DeleteAsync(id);

    public static CreateGraphRelationshipDto ToDto(CreateGraphRelationshipRequest request) => new()
    {
        SourceEntityId = request.SourceEntityId,
        TargetEntityId = request.TargetEntityId,
        Predicate = request.Predicate,
        Confidence = request.Confidence
    };

    public static UpdateGraphRelationshipDto ToDto(UpdateGraphRelationshipRequest request) => new()
    {
        SourceEntityId = request.SourceEntityId,
        TargetEntityId = request.TargetEntityId,
        Predicate = request.Predicate,
        Confidence = request.Confidence
    };
}
