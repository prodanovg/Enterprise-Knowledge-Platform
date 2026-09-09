using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.ApiKeys;

namespace Web.Extensions;

public static class ApiKeyExtensions
{
    public static ApiKeyResponse ToResponse(this ApiKey apiKey) => new()
    {
        Id = apiKey.Id,
        Label = apiKey.Label,
        ExpiresAt = apiKey.ExpiresAt,
        IsActive = apiKey.IsActive,
        CreatedAt = apiKey.CreatedAt
    };

    public static List<ApiKeyResponse> ToResponse(this IEnumerable<ApiKey> apiKeys) =>
        apiKeys.Select(x => x.ToResponse()).ToList();

    public static PaginatedResponse<ApiKeyResponse> ToResponse(
        this PaginatedResult<ApiKey> result) => new()
    {
        Items = result.Items.ToResponse(),
        TotalCount = result.TotalCount,
        PageNumber = result.PageNumber,
        PageSize = result.PageSize,
        TotalPages = result.TotalPages
    };
}
