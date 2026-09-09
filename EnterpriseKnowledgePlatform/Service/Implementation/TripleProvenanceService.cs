using Domain.Dto;
using Domain.Dto.TripleProvenance;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class TripleProvenanceService : ITripleProvenanceService
{
    private readonly IRepository<TripleProvenance> _tripleProvenanceRepository;
    private readonly IRepository<Triple> _tripleRepository;
    private readonly IRepository<Document> _documentRepository;
    private readonly IRepository<SemanticBlock> _semanticBlockRepository;

    public TripleProvenanceService(
        IRepository<TripleProvenance> tripleProvenanceRepository,
        IRepository<Triple> tripleRepository,
        IRepository<Document> documentRepository,
        IRepository<SemanticBlock> semanticBlockRepository)
    {
        _tripleProvenanceRepository = tripleProvenanceRepository;
        _tripleRepository = tripleRepository;
        _documentRepository = documentRepository;
        _semanticBlockRepository = semanticBlockRepository;
    }

    public async Task<TripleProvenance> CreateAsync(
        CreateTripleProvenanceDto dto, string userId)
    {
        await ValidateRelationshipsAsync(
            dto.TripleId, dto.DocumentId, dto.SemanticBlockId, userId);

        var now = DateTime.UtcNow;
        var provenance = new TripleProvenance
        {
            TripleId = dto.TripleId,
            DocumentId = dto.DocumentId,
            SemanticBlockId = dto.SemanticBlockId,
            CreatedAt = now,
            CreatedBy = userId,
            ModifiedAt = now,
            ModifiedBy = userId
        };

        await _tripleProvenanceRepository.InsertAsync(provenance);
        await _tripleProvenanceRepository.SaveChangesAsync();

        return provenance;
    }

    public Task<TripleProvenance?> GetByIdAsync(Guid id, string userId)
    {
        return _tripleProvenanceRepository.GetAsync<TripleProvenance>(
            selector: x => x,
            predicate: x => x.Id == id && x.Document.OwnerId == userId,
            asNoTracking: true);
    }

    public Task<List<TripleProvenance>> GetAllAsync(string userId)
    {
        return _tripleProvenanceRepository.GetAllAsync<TripleProvenance>(
            selector: x => x,
            predicate: x => x.Document.OwnerId == userId,
            orderBy: x => x.OrderByDescending(provenance => provenance.CreatedAt));
    }

    public async Task<PaginatedResult<TripleProvenance>> GetAllPagedAsync(
        int pageNumber, int pageSize, string userId)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException(
                "Page number must be greater than or equal to 1.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentException(
                "Page size must be greater than zero.");
        }

        return await _tripleProvenanceRepository
            .GetAllPagedAsync<TripleProvenance>(
                selector: x => x,
                pageNumber: pageNumber,
                pageSize: pageSize,
                predicate: x => x.Document.OwnerId == userId,
                orderBy: x => x.OrderByDescending(
                    provenance => provenance.CreatedAt),
                asNoTracking: true);
    }

    public async Task<TripleProvenance> UpdateAsync(
        Guid id, UpdateTripleProvenanceDto dto, string userId)
    {
        var provenance = await _tripleProvenanceRepository
            .GetAsync<TripleProvenance>(
                selector: x => x,
                predicate: x => x.Id == id && x.Document.OwnerId == userId);

        if (provenance == null)
        {
            throw new KeyNotFoundException(
                $"Triple provenance with ID '{id}' was not found.");
        }

        await ValidateRelationshipsAsync(
            dto.TripleId, dto.DocumentId, dto.SemanticBlockId, userId);

        provenance.TripleId = dto.TripleId;
        provenance.DocumentId = dto.DocumentId;
        provenance.SemanticBlockId = dto.SemanticBlockId;
        provenance.ModifiedAt = DateTime.UtcNow;
        provenance.ModifiedBy = userId;

        await _tripleProvenanceRepository.UpdateAsync(provenance);
        await _tripleProvenanceRepository.SaveChangesAsync();

        return provenance;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var provenance = await _tripleProvenanceRepository
            .GetAsync<TripleProvenance>(
                selector: x => x,
                predicate: x => x.Id == id && x.Document.OwnerId == userId);

        if (provenance == null)
        {
            return false;
        }

        await _tripleProvenanceRepository.DeleteAsync(provenance);
        await _tripleProvenanceRepository.SaveChangesAsync();

        return true;
    }

    private async Task ValidateRelationshipsAsync(
        Guid tripleId,
        Guid documentId,
        Guid semanticBlockId,
        string userId)
    {
        var document = await _documentRepository.GetAsync<Document>(
            selector: x => x,
            predicate: x => x.Id == documentId && x.OwnerId == userId);

        if (document == null)
        {
            throw new KeyNotFoundException(
                $"Document with ID '{documentId}' was not found.");
        }

        var triple = await _tripleRepository.GetAsync<Triple>(
            selector: x => x,
            predicate: x => x.Id == tripleId);

        if (triple == null)
        {
            throw new KeyNotFoundException(
                $"Triple with ID '{tripleId}' was not found.");
        }

        var semanticBlock = await _semanticBlockRepository
            .GetAsync<SemanticBlock>(
                selector: x => x,
                predicate: x =>
                    x.Id == semanticBlockId &&
                    x.DocumentId == documentId);

        if (semanticBlock == null)
        {
            throw new KeyNotFoundException(
                $"Semantic block with ID '{semanticBlockId}' was not found " +
                "for the specified document.");
        }
    }
}
