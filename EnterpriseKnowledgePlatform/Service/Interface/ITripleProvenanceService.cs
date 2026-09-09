using Domain.Dto;
using Domain.Dto.TripleProvenance;
using Domain.Models;

namespace Service.Interface;

public interface ITripleProvenanceService
{
    Task<TripleProvenance> CreateAsync(
        CreateTripleProvenanceDto dto, string userId);
    Task<TripleProvenance?> GetByIdAsync(Guid id, string userId);
    Task<List<TripleProvenance>> GetAllAsync(string userId);
    Task<PaginatedResult<TripleProvenance>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId);
    Task<TripleProvenance> UpdateAsync(
        Guid id, UpdateTripleProvenanceDto dto, string userId);
    Task<bool> DeleteAsync(Guid id, string userId);
}
