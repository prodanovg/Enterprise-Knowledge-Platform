using Domain.Dto;
using Domain.Dto.ApiKeys;
using Domain.Models;

namespace Service.Interface;

public interface IApiKeyService
{
    Task<ApiKeyCreationResult> CreateAsync(CreateApiKeyDto dto, string userId);
    Task<ApiKey?> ValidateAsync(string plaintextKey);
    Task<ApiKey?> GetByIdAsync(Guid id, string userId);
    Task<List<ApiKey>> GetAllAsync(string userId);
    Task<PaginatedResult<ApiKey>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId);
    Task<ApiKey> UpdateAsync(Guid id, UpdateApiKeyDto dto, string userId);
    Task<bool> DeleteAsync(Guid id, string userId);
}
