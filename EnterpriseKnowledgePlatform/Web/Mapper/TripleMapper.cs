using Domain.Dto.Triples;
using Service.Interface;
using Web.Extensions;
using Web.Request.Triples;
using Web.Response;
using Web.Response.Triples;

namespace Web.Mapper;

public class TripleMapper
{
    private readonly ITripleService _tripleService;

    public TripleMapper(ITripleService tripleService)
    {
        _tripleService = tripleService;
    }

    public async Task<TripleResponse> CreateAsync(CreateTripleRequest request)
    {
        return (await _tripleService.CreateAsync(ToDto(request))).ToResponse();
    }

    public async Task<TripleResponse?> GetByIdAsync(Guid id)
    {
        return (await _tripleService.GetByIdAsync(id))?.ToResponse();
    }

    public async Task<List<TripleResponse>> GetAllAsync()
    {
        return (await _tripleService.GetAllAsync()).ToResponse();
    }

    public async Task<PaginatedResponse<TripleResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize)
    {
        return (await _tripleService.GetAllPagedAsync(pageNumber, pageSize)).ToResponse();
    }

    public async Task<TripleResponse> UpdateAsync(Guid id, UpdateTripleRequest request)
    {
        return (await _tripleService.UpdateAsync(id, ToDto(request))).ToResponse();
    }

    public Task<bool> DeleteAsync(Guid id) => _tripleService.DeleteAsync(id);

    public static CreateTripleDto ToDto(CreateTripleRequest request) => new()
    {
        Subject = request.Subject,
        Predicate = request.Predicate,
        Object = request.Object,
        Confidence = request.Confidence,
        Status = request.Status
    };

    public static UpdateTripleDto ToDto(UpdateTripleRequest request) => new()
    {
        Subject = request.Subject,
        Predicate = request.Predicate,
        Object = request.Object,
        Confidence = request.Confidence,
        Status = request.Status
    };
}
