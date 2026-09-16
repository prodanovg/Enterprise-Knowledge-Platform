using System.Security.Cryptography;
using Domain.Dto;
using Domain.Dto.ApiKeys;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class ApiKeyService : IApiKeyService
{
    private const int KeySize = 32;
    private const int SaltSize = 16;
    private const int Iterations = 100_000;
    private readonly IRepository<ApiKey> _apiKeyRepository;

    public ApiKeyService(IRepository<ApiKey> apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<ApiKeyCreationResult> CreateAsync(
        CreateApiKeyDto dto, string userId)
    {
        var plaintextKey = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(KeySize))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        var now = DateTime.UtcNow;
        var apiKey = new ApiKey
        {
            UserId = userId,
            KeyHash = HashKey(plaintextKey),
            Label = dto.Label,
            ExpiresAt = dto.ExpiresAt,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = userId,
            ModifiedAt = now,
            ModifiedBy = userId
        };

        await _apiKeyRepository.InsertAsync(apiKey);
        await _apiKeyRepository.SaveChangesAsync();

        return new ApiKeyCreationResult
        {
            ApiKey = apiKey,
            PlaintextKey = plaintextKey
        };
    }

    public async Task<ApiKey?> ValidateAsync(string plaintextKey)
    {
        if (string.IsNullOrWhiteSpace(plaintextKey)) return null;

        var keys = await _apiKeyRepository.GetAllAsync<ApiKey>(x => x,
            x => x.IsActive && (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow));
        return keys.FirstOrDefault(key => VerifyKey(plaintextKey, key.KeyHash));
    }

    public Task<ApiKey?> GetByIdAsync(Guid id, string userId)
    {
        return _apiKeyRepository.GetAsync<ApiKey>(x => x,
            x => x.Id == id && x.UserId == userId,
            asNoTracking: true);
    }

    public Task<List<ApiKey>> GetAllAsync(string userId)
    {
        return _apiKeyRepository.GetAllAsync<ApiKey>(x => x,
            x => x.UserId == userId,
            x => x.OrderByDescending(apiKey => apiKey.CreatedAt));
    }

    public async Task<PaginatedResult<ApiKey>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than or equal to 1.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentException("Page size must be greater than zero.");
        }

        return await _apiKeyRepository.GetAllPagedAsync<ApiKey>(
            x => x, pageNumber, pageSize,
            x => x.UserId == userId,
            x => x.OrderByDescending(apiKey => apiKey.CreatedAt),
            asNoTracking: true);
    }

    public async Task<ApiKey> UpdateAsync(
        Guid id, UpdateApiKeyDto dto, string userId)
    {
        var apiKey = await _apiKeyRepository.GetAsync<ApiKey>(
            x => x, x => x.Id == id && x.UserId == userId);

        if (apiKey == null)
        {
            throw new KeyNotFoundException($"API key with ID '{id}' was not found.");
        }

        apiKey.Label = dto.Label;
        apiKey.ExpiresAt = dto.ExpiresAt;
        apiKey.IsActive = dto.IsActive;
        apiKey.ModifiedAt = DateTime.UtcNow;
        apiKey.ModifiedBy = userId;

        await _apiKeyRepository.UpdateAsync(apiKey);
        await _apiKeyRepository.SaveChangesAsync();
        return apiKey;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var apiKey = await _apiKeyRepository.GetAsync<ApiKey>(
            x => x, x => x.Id == id && x.UserId == userId);

        if (apiKey == null)
        {
            return false;
        }

        await _apiKeyRepository.DeleteAsync(apiKey);
        await _apiKeyRepository.SaveChangesAsync();
        return true;
    }

    private static string HashKey(string plaintextKey)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            plaintextKey, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"v1${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerifyKey(string plaintextKey, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != "v1" ||
            !int.TryParse(parts[1], out var iterations)) return false;

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(
                plaintextKey, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
