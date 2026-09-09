using Domain.Dto.TripleProvenance;
using Service.Interface;
using Web.Extensions;
using Web.Request.TripleProvenance;
using Web.Response;
using Web.Response.TripleProvenance;

namespace Web.Mapper;

public class TripleProvenanceMapper
{
    private readonly ITripleProvenanceService _tripleProvenanceService;

    public TripleProvenanceMapper(
        ITripleProvenanceService tripleProvenanceService)
    {
        _tripleProvenanceService = tripleProvenanceService;
    }

    public async Task<TripleProvenanceResponse> CreateAsync(
        CreateTripleProvenanceRequest request, string userId)
    {
        var provenance = await _tripleProvenanceService
            .CreateAsync(ToDto(request), userId);
        return provenance.ToResponse();
    }

    public async Task<TripleProvenanceResponse?> GetByIdAsync(
        Guid id, string userId)
    {
        var provenance = await _tripleProvenanceService
            .GetByIdAsync(id, userId);
        return provenance?.ToResponse();
    }

    public async Task<List<TripleProvenanceResponse>> GetAllAsync(
        string userId)
    {
        var provenances = await _tripleProvenanceService.GetAllAsync(userId);
        return provenances.ToResponse();
    }

    public async Task<PaginatedResponse<TripleProvenanceResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId)
    {
        var provenances = await _tripleProvenanceService
            .GetAllPagedAsync(pageNumber, pageSize, userId);
        return provenances.ToResponse();
    }

    public async Task<TripleProvenanceResponse> UpdateAsync(
        Guid id, UpdateTripleProvenanceRequest request, string userId)
    {
        var provenance = await _tripleProvenanceService
            .UpdateAsync(id, ToDto(request), userId);
        return provenance.ToResponse();
    }

    public Task<bool> DeleteAsync(Guid id, string userId)
    {
        return _tripleProvenanceService.DeleteAsync(id, userId);
    }

    public static CreateTripleProvenanceDto ToDto(
        CreateTripleProvenanceRequest request)
    {
        return new CreateTripleProvenanceDto
        {
            TripleId = request.TripleId,
            DocumentId = request.DocumentId,
            SemanticBlockId = request.SemanticBlockId
        };
    }

    public static UpdateTripleProvenanceDto ToDto(
        UpdateTripleProvenanceRequest request)
    {
        return new UpdateTripleProvenanceDto
        {
            TripleId = request.TripleId,
            DocumentId = request.DocumentId,
            SemanticBlockId = request.SemanticBlockId
        };
    }
}
