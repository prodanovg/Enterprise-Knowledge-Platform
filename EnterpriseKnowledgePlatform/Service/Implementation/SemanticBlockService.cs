using Domain.Dto;
using Domain.Dto.SemanticBlocks;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class SemanticBlockService : ISemanticBlockService
{
    private readonly IRepository<SemanticBlock> _semanticBlockRepository;
    private readonly IRepository<Document> _documentRepository;

    public SemanticBlockService(
        IRepository<SemanticBlock> semanticBlockRepository,
        IRepository<Document> documentRepository)
    {
        _semanticBlockRepository = semanticBlockRepository;
        _documentRepository = documentRepository;
    }

    public async Task<SemanticBlock> CreateAsync(
        CreateSemanticBlockDto dto, string userId)
    {
        var document = await _documentRepository.GetAsync<Document>(
            selector: x => x,
            predicate: x => x.Id == dto.DocumentId && x.OwnerId == userId);

        if (document == null)
        {
            throw new KeyNotFoundException(
                $"Document with ID '{dto.DocumentId}' was not found.");
        }

        var now = DateTime.UtcNow;
        var semanticBlock = new SemanticBlock
        {
            DocumentId = dto.DocumentId,
            Text = dto.Text,
            BlockIndex = dto.BlockIndex,
            Page = dto.Page,
            CreatedAt = now,
            CreatedBy = userId,
            ModifiedAt = now,
            ModifiedBy = userId
        };

        await _semanticBlockRepository.InsertAsync(semanticBlock);
        await _semanticBlockRepository.SaveChangesAsync();

        return semanticBlock;
    }

    public Task<SemanticBlock?> GetByIdAsync(Guid id, string userId)
    {
        return _semanticBlockRepository.GetAsync<SemanticBlock>(
            selector: x => x,
            predicate: x => x.Id == id && x.Document.OwnerId == userId,
            asNoTracking: true);
    }

    public Task<List<SemanticBlock>> GetAllAsync(string userId)
    {
        return _semanticBlockRepository.GetAllAsync<SemanticBlock>(
            selector: x => x,
            predicate: x => x.Document.OwnerId == userId,
            orderBy: x => x.OrderByDescending(block => block.CreatedAt));
    }

    public async Task<PaginatedResult<SemanticBlock>> GetAllPagedAsync(
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

        return await _semanticBlockRepository
            .GetAllPagedAsync<SemanticBlock>(
                selector: x => x,
                pageNumber: pageNumber,
                pageSize: pageSize,
                predicate: x => x.Document.OwnerId == userId,
                orderBy: x => x.OrderByDescending(block => block.CreatedAt),
                asNoTracking: true);
    }

    public async Task<SemanticBlock> UpdateAsync(
        Guid id, UpdateSemanticBlockDto dto, string userId)
    {
        var semanticBlock = await _semanticBlockRepository
            .GetAsync<SemanticBlock>(
                selector: x => x,
                predicate: x => x.Id == id && x.Document.OwnerId == userId);

        if (semanticBlock == null)
        {
            throw new KeyNotFoundException(
                $"Semantic block with ID '{id}' was not found.");
        }

        semanticBlock.Text = dto.Text;
        semanticBlock.BlockIndex = dto.BlockIndex;
        semanticBlock.Page = dto.Page;
        semanticBlock.ModifiedAt = DateTime.UtcNow;
        semanticBlock.ModifiedBy = userId;

        await _semanticBlockRepository.UpdateAsync(semanticBlock);
        await _semanticBlockRepository.SaveChangesAsync();

        return semanticBlock;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var semanticBlock = await _semanticBlockRepository
            .GetAsync<SemanticBlock>(
                selector: x => x,
                predicate: x => x.Id == id && x.Document.OwnerId == userId);

        if (semanticBlock == null)
        {
            return false;
        }

        await _semanticBlockRepository.DeleteAsync(semanticBlock);
        await _semanticBlockRepository.SaveChangesAsync();

        return true;
    }
}

