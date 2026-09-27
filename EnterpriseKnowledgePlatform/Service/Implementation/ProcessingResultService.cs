using Domain.Dto.ProcessingResults; using Domain.Enums; using Domain.Models; using Microsoft.EntityFrameworkCore; using Repository; using Service.Interface;
namespace Service.Implementation;
public class ProcessingResultService : IProcessingResultService
{
 private readonly ApplicationDbContext _db; public ProcessingResultService(ApplicationDbContext db) => _db = db;
 public async Task<ProcessingResultSummary> PersistAsync(Guid id, ProcessingResultDto result, CancellationToken token = default)
 {
  var job = await _db.ProcessingJobs.Include(x => x.Document).SingleOrDefaultAsync(x => x.Id == id, token); if (job == null) throw new KeyNotFoundException();
  if (job.Status == ProcessingJobStatus.Completed) return new() { ProcessingJobId = id, Status = job.Status.ToString() };
  if (job.Status != ProcessingJobStatus.Processing || job.Document == null) throw new InvalidOperationException();
  var now = DateTime.UtcNow; var blocks = result.SemanticBlocks.Select(x => new SemanticBlock { DocumentId = job.DocumentId, Text = x.Text, BlockIndex = x.BlockIndex, Page = x.Page, CreatedAt = now, ModifiedAt = now }).ToList(); var indexes = blocks.ToDictionary(x => x.BlockIndex);
  if (result.Triples.Any(x => !indexes.ContainsKey(x.SemanticBlockIndex))) throw new ArgumentException();
  await using var transaction = await _db.Database.BeginTransactionAsync(token); _db.SemanticBlocks.AddRange(blocks); await _db.SaveChangesAsync(token);
  var triples = result.Triples.Select(x => new Triple { Subject = x.Subject, Predicate = x.Predicate, Object = x.Object, Confidence = x.Confidence, Status = x.Status, CreatedAt = now, ModifiedAt = now }).ToList(); _db.Triples.AddRange(triples); await _db.SaveChangesAsync(token);
  await PersistGraphAsync(result.Triples, now, token);
  _db.TripleProvenances.AddRange(triples.Zip(result.Triples, (t, x) => new TripleProvenance { TripleId = t.Id, DocumentId = job.DocumentId, SemanticBlockId = indexes[x.SemanticBlockIndex].Id, CreatedAt = now, ModifiedAt = now })); job.Status = ProcessingJobStatus.Completed; job.FinishedAt = DateTime.UtcNow; job.ModifiedAt = job.FinishedAt.Value; await _db.SaveChangesAsync(token); await transaction.CommitAsync(token);
  return new() { ProcessingJobId = id, Status = job.Status.ToString(), SemanticBlocks = blocks.Count, Triples = triples.Count };
 }

 private async Task PersistGraphAsync(IEnumerable<ProcessingTripleDto> triples, DateTime now, CancellationToken token)
 {
  var entityType = await _db.EntityTypes.FirstOrDefaultAsync(x => x.Name == "Entity", token);
  if (entityType == null)
  {
   entityType = new EntityType { Id = Guid.NewGuid(), Name = "Entity", CreatedAt = now, ModifiedAt = now };
   _db.EntityTypes.Add(entityType);
   await _db.SaveChangesAsync(token);
  }

  var entities = new Dictionary<string, GraphEntity>(StringComparer.OrdinalIgnoreCase);
  var relationships = new HashSet<string>(StringComparer.Ordinal);
  foreach (var triple in triples)
  {
   var source = await GetOrCreateEntityAsync(triple.Subject, entityType.Id, entityType, entities, now, token);
   var target = await GetOrCreateEntityAsync(triple.Object, entityType.Id, entityType, entities, now, token);
   var predicate = triple.Predicate.Trim();
   var relationshipKey = $"{source.Id:N}:{target.Id:N}:{predicate}";
   var exists = relationships.Contains(relationshipKey) || await _db.GraphRelationships.AnyAsync(x =>
       x.SourceEntityId == source.Id && x.TargetEntityId == target.Id && x.Predicate == predicate, token);
   if (!exists)
   {
    _db.GraphRelationships.Add(new GraphRelationship
    {
     Id = Guid.NewGuid(), SourceEntityId = source.Id, TargetEntityId = target.Id,
     Predicate = predicate, Confidence = triple.Confidence, CreatedAt = now, ModifiedAt = now
    });
    relationships.Add(relationshipKey);
   }
  }
  await _db.SaveChangesAsync(token);
 }

 private async Task<GraphEntity> GetOrCreateEntityAsync(string name, Guid entityTypeId, EntityType entityType, Dictionary<string, GraphEntity> cache, DateTime now, CancellationToken token)
 {
  var normalized = name.Trim();
  var key = normalized.ToLowerInvariant();
  if (cache.TryGetValue(key, out var cached)) return cached;
  var entity = await _db.GraphEntities.FirstOrDefaultAsync(x => x.CanonicalName == key, token);
  if (entity == null)
  {
   entity = new GraphEntity { Id = Guid.NewGuid(), Name = normalized, CanonicalName = key, EntityTypeId = entityTypeId, EntityType = entityType, CreatedAt = now, ModifiedAt = now };
   _db.GraphEntities.Add(entity);
   await _db.SaveChangesAsync(token);
  }
  cache[key] = entity;
  return entity;
 }
}
