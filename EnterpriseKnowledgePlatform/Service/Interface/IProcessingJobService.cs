using Domain.Dto;
using Domain.Dto.ProcessingJobs;
using Domain.Models;

namespace Service.Interface;

public interface IProcessingJobService
{
    Task<ProcessingJob> CreateAsync(CreateProcessingJobDto dto, string userId);
    Task<ProcessingJob?> GetByIdAsync(Guid id, string userId);
    Task<List<ProcessingJob>> GetAllAsync(string userId);
    Task<PaginatedResult<ProcessingJob>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId);
    Task<ProcessingJob> UpdateAsync(
        Guid id, UpdateProcessingJobDto dto, string userId);
    Task<bool> DeleteAsync(Guid id, string userId);
}

