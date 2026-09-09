using Domain.Dto.ApiKeys;
using Service.Interface;
using Web.Extensions;
using Web.Request.ApiKeys;
using Web.Response;
using Web.Response.ApiKeys;

namespace Web.Mapper;

public class ApiKeyMapper
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyMapper(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<CreateApiKeyResponse> CreateAsync(
        CreateApiKeyRequest request, string userId)
    {
        var result = await _apiKeyService.CreateAsync(ToDto(request), userId);
        var response = result.ApiKey.ToResponse();
        return new CreateApiKeyResponse
        {
            Id = response.Id,
            Label = response.Label,
            ExpiresAt = response.ExpiresAt,
            IsActive = response.IsActive,
            CreatedAt = response.CreatedAt,
            ApiKey = result.PlaintextKey
        };
    }

    public async Task<ApiKeyResponse?> GetByIdAsync(Guid id, string userId) =>
        (await _apiKeyService.GetByIdAsync(id, userId))?.ToResponse();

    public async Task<List<ApiKeyResponse>> GetAllAsync(string userId) =>
        (await _apiKeyService.GetAllAsync(userId)).ToResponse();

    public async Task<PaginatedResponse<ApiKeyResponse>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId) =>
        (await _apiKeyService.GetAllPagedAsync(pageNumber, pageSize, userId)).ToResponse();

    public async Task<ApiKeyResponse> UpdateAsync(
        Guid id, UpdateApiKeyRequest request, string userId) =>
        (await _apiKeyService.UpdateAsync(id, ToDto(request), userId)).ToResponse();

    public Task<bool> DeleteAsync(Guid id, string userId) =>
        _apiKeyService.DeleteAsync(id, userId);

    public static CreateApiKeyDto ToDto(CreateApiKeyRequest request) => new()
    {
        Label = request.Label,
        ExpiresAt = request.ExpiresAt
    };

    public static UpdateApiKeyDto ToDto(UpdateApiKeyRequest request) => new()
    {
        Label = request.Label,
        ExpiresAt = request.ExpiresAt,
        IsActive = request.IsActive
    };
}
