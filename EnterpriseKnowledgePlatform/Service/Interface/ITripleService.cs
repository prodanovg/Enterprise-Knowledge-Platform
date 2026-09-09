using Domain.Dto;
using Domain.Dto.Triples;
using Domain.Models;

namespace Service.Interface;

public interface ITripleService
{
    Task<Triple> CreateAsync(CreateTripleDto dto);
    Task<Triple?> GetByIdAsync(Guid id);
    Task<List<Triple>> GetAllAsync();
    Task<PaginatedResult<Triple>> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<Triple> UpdateAsync(Guid id, UpdateTripleDto dto);
    Task<bool> DeleteAsync(Guid id);
}
