using Domain.Dto;
using Domain.Dto.Triples;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class TripleService : ITripleService
{
    private readonly IRepository<Triple> _tripleRepository;

    public TripleService(IRepository<Triple> tripleRepository)
    {
        _tripleRepository = tripleRepository;
    }

    public async Task<Triple> CreateAsync(CreateTripleDto dto)
    {
        var now = DateTime.UtcNow;
        var triple = new Triple
        {
            Subject = dto.Subject,
            Predicate = dto.Predicate,
            Object = dto.Object,
            Confidence = dto.Confidence,
            Status = dto.Status,
            CreatedAt = now,
            ModifiedAt = now
        };

        await _tripleRepository.InsertAsync(triple);
        await _tripleRepository.SaveChangesAsync();
        return triple;
    }

    public Task<Triple?> GetByIdAsync(Guid id)
    {
        return _tripleRepository.GetAsync<Triple>(x => x,
            predicate: x => x.Id == id,
            asNoTracking: true);
    }

    public Task<List<Triple>> GetAllAsync()
    {
        return _tripleRepository.GetAllAsync<Triple>(x => x,
            orderBy: x => x.OrderByDescending(triple => triple.CreatedAt));
    }

    public async Task<PaginatedResult<Triple>> GetAllPagedAsync(
        int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than or equal to 1.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentException("Page size must be greater than zero.");
        }

        return await _tripleRepository.GetAllPagedAsync<Triple>(
            x => x,
            pageNumber,
            pageSize,
            orderBy: x => x.OrderByDescending(triple => triple.CreatedAt),
            asNoTracking: true);
    }

    public async Task<Triple> UpdateAsync(Guid id, UpdateTripleDto dto)
    {
        var triple = await _tripleRepository.GetAsync<Triple>(
            x => x, x => x.Id == id);

        if (triple == null)
        {
            throw new KeyNotFoundException($"Triple with ID '{id}' was not found.");
        }

        triple.Subject = dto.Subject;
        triple.Predicate = dto.Predicate;
        triple.Object = dto.Object;
        triple.Confidence = dto.Confidence;
        triple.Status = dto.Status;
        triple.ModifiedAt = DateTime.UtcNow;

        await _tripleRepository.UpdateAsync(triple);
        await _tripleRepository.SaveChangesAsync();
        return triple;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var triple = await _tripleRepository.GetAsync<Triple>(
            x => x, x => x.Id == id);

        if (triple == null)
        {
            return false;
        }

        await _tripleRepository.DeleteAsync(triple);
        await _tripleRepository.SaveChangesAsync();
        return true;
    }
}
