using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.Triples;

namespace Web.Extensions;

public static class TripleExtensions
{
    public static TripleResponse ToResponse(this Triple triple) => new()
    {
        Id = triple.Id,
        Subject = triple.Subject,
        Predicate = triple.Predicate,
        Object = triple.Object,
        Confidence = triple.Confidence,
        Status = triple.Status,
        CreatedAt = triple.CreatedAt
    };

    public static List<TripleResponse> ToResponse(this IEnumerable<Triple> triples) =>
        triples.Select(x => x.ToResponse()).ToList();

    public static PaginatedResponse<TripleResponse> ToResponse(
        this PaginatedResult<Triple> result) => new()
    {
        Items = result.Items.ToResponse(),
        TotalCount = result.TotalCount,
        PageNumber = result.PageNumber,
        PageSize = result.PageSize,
        TotalPages = result.TotalPages
    };
}
