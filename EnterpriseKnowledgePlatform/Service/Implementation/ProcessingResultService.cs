using Domain.Dto.ProcessingResults;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service.Interface;

namespace Service.Implementation;

public class ProcessingResultService : IProcessingResultService
{
    private readonly ApplicationDbContext _db;

    public ProcessingResultService(ApplicationDbContext db) => _db = db;

    public async Task<ProcessingResultSummary> PersistAsync(
        Guid processingJobId,
        ProcessingResultDto result,
        CancellationToken cancellationToken = default)
    {
        var job = await _db.ProcessingJobs
            .Include(x => x.Document)
            .SingleOrDefaultAsync(x => x.Id == processingJobId, cancellationToken);

        if (job == null)
        {
            throw new KeyNotFoundException();
        }

        if (job.Status == ProcessingJobStatus.Completed)
        {
            return new ProcessingResultSummary
            {
                ProcessingJobId = processingJobId,
                Status = job.Status.ToString()
            };
        }

        if (job.Status != ProcessingJobStatus.Processing || job.Document == null)
        {
            throw new InvalidOperationException();
        }

        var now = DateTime.UtcNow;
        var semanticBlocks = result.SemanticBlocks
            .Select(x => new SemanticBlock
            {
                DocumentId = job.DocumentId,
                Text = x.Text,
                BlockIndex = x.BlockIndex,
                Page = x.Page,
                CreatedAt = now,
                ModifiedAt = now
            })
            .ToList();
        var blocksByIndex = semanticBlocks.ToDictionary(x => x.BlockIndex);

        if (result.Triples.Any(x => !blocksByIndex.ContainsKey(x.SemanticBlockIndex)))
        {
            throw new ArgumentException();
        }

        await using var transaction = await _db.Database
            .BeginTransactionAsync(cancellationToken);

        _db.SemanticBlocks.AddRange(semanticBlocks);
        await _db.SaveChangesAsync(cancellationToken);

        var triples = result.Triples
            .Select(x => new Triple
            {
                Subject = x.Subject,
                Predicate = x.Predicate,
                Object = x.Object,
                Confidence = x.Confidence,
                Status = x.Status,
                CreatedAt = now,
                ModifiedAt = now
            })
            .ToList();

        _db.Triples.AddRange(triples);
        await _db.SaveChangesAsync(cancellationToken);
        await PersistGraphAsync(result.Triples, now, cancellationToken);

        _db.TripleProvenances.AddRange(
            triples.Zip(result.Triples, (triple, sourceTriple) => new TripleProvenance
            {
                TripleId = triple.Id,
                DocumentId = job.DocumentId,
                SemanticBlockId = blocksByIndex[sourceTriple.SemanticBlockIndex].Id,
                CreatedAt = now,
                ModifiedAt = now
            }));

        job.Status = ProcessingJobStatus.Completed;
        job.FinishedAt = DateTime.UtcNow;
        job.ModifiedAt = job.FinishedAt.Value;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ProcessingResultSummary
        {
            ProcessingJobId = processingJobId,
            Status = job.Status.ToString(),
            SemanticBlocks = semanticBlocks.Count,
            Triples = triples.Count
        };
    }

    private async Task PersistGraphAsync(
        IEnumerable<ProcessingTripleDto> triples,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var entityType = await _db.EntityTypes
            .FirstOrDefaultAsync(x => x.Name == "Entity", cancellationToken);
        if (entityType == null)
        {
            entityType = new EntityType
            {
                Id = Guid.NewGuid(),
                Name = "Entity",
                CreatedAt = now,
                ModifiedAt = now
            };
            _db.EntityTypes.Add(entityType);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var entities = new Dictionary<string, GraphEntity>(StringComparer.OrdinalIgnoreCase);
        var relationships = new HashSet<string>(StringComparer.Ordinal);
        foreach (var triple in triples)
        {
            var source = await GetOrCreateEntityAsync(
                triple.Subject, entityType.Id, entityType, entities, now, cancellationToken);
            var target = await GetOrCreateEntityAsync(
                triple.Object, entityType.Id, entityType, entities, now, cancellationToken);
            var predicate = triple.Predicate.Trim();
            var relationshipKey = $"{source.Id:N}:{target.Id:N}:{predicate}";
            var exists = relationships.Contains(relationshipKey) ||
                         await _db.GraphRelationships.AnyAsync(x =>
                             x.SourceEntityId == source.Id &&
                             x.TargetEntityId == target.Id &&
                             x.Predicate == predicate,
                             cancellationToken);

            if (!exists)
            {
                _db.GraphRelationships.Add(new GraphRelationship
                {
                    Id = Guid.NewGuid(),
                    SourceEntityId = source.Id,
                    TargetEntityId = target.Id,
                    Predicate = predicate,
                    Confidence = triple.Confidence,
                    CreatedAt = now,
                    ModifiedAt = now
                });
                relationships.Add(relationshipKey);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<GraphEntity> GetOrCreateEntityAsync(
        string name,
        Guid entityTypeId,
        EntityType entityType,
        Dictionary<string, GraphEntity> cache,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var normalized = name.Trim();
        var key = normalized.ToLowerInvariant();
        if (cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var entity = await _db.GraphEntities
            .FirstOrDefaultAsync(x => x.CanonicalName == key, cancellationToken);
        if (entity == null)
        {
            entity = new GraphEntity
            {
                Id = Guid.NewGuid(),
                Name = normalized,
                CanonicalName = key,
                EntityTypeId = entityTypeId,
                EntityType = entityType,
                CreatedAt = now,
                ModifiedAt = now
            };
            _db.GraphEntities.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        cache[key] = entity;
        return entity;
    }
}
