using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.TripleProvenance;

namespace Web.Extensions;

public static class TripleProvenanceExtensions
{
    public static TripleProvenanceResponse ToResponse(
        this TripleProvenance provenance)
    {
        return new TripleProvenanceResponse
        {
            Id = provenance.Id,
            TripleId = provenance.TripleId,
            DocumentId = provenance.DocumentId,
            SemanticBlockId = provenance.SemanticBlockId,
            CreatedAt = provenance.CreatedAt
        };
    }

    public static List<TripleProvenanceResponse> ToResponse(
        this IEnumerable<TripleProvenance> provenances)
    {
        return provenances
            .Select(provenance => provenance.ToResponse())
            .ToList();
    }

    public static PaginatedResponse<TripleProvenanceResponse> ToResponse(
        this PaginatedResult<TripleProvenance> result)
    {
        return new PaginatedResponse<TripleProvenanceResponse>
        {
            Items = result.Items.ToResponse(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }
}
