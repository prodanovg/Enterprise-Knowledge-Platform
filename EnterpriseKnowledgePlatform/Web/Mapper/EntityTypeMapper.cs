using Domain.Dto.EntityTypes;
using Service.Interface;
using Web.Extensions;
using Web.Request.EntityTypes;
using Web.Response;
using Web.Response.EntityTypes;

namespace Web.Mapper;

public class EntityTypeMapper
{
    private readonly IEntityTypeService _entityTypeService;

    public EntityTypeMapper(IEntityTypeService entityTypeService)
    {
        _entityTypeService = entityTypeService;
    }

    public async Task<EntityTypeResponse> CreateAsync(CreateEntityTypeRequest request) =>
        (await _entityTypeService.CreateAsync(ToDto(request))).ToResponse();

    public async Task<EntityTypeResponse?> GetByIdAsync(Guid id) =>
        (await _entityTypeService.GetByIdAsync(id))?.ToResponse();

    public async Task<List<EntityTypeResponse>> GetAllAsync() =>
        (await _entityTypeService.GetAllAsync()).ToResponse();

    public async Task<PaginatedResponse<EntityTypeResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize) =>
        (await _entityTypeService.GetAllPagedAsync(pageNumber, pageSize)).ToResponse();

    public async Task<EntityTypeResponse> UpdateAsync(
        Guid id, UpdateEntityTypeRequest request) =>
        (await _entityTypeService.UpdateAsync(id, ToDto(request))).ToResponse();

    public Task<bool> DeleteAsync(Guid id) => _entityTypeService.DeleteAsync(id);

    public static CreateEntityTypeDto ToDto(CreateEntityTypeRequest request) => new()
    { Name = request.Name };

    public static UpdateEntityTypeDto ToDto(UpdateEntityTypeRequest request) => new()
    { Name = request.Name };
}
