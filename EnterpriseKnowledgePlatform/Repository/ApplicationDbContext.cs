using Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<ProcessingJob> ProcessingJobs { get; set; }
    public DbSet<SemanticBlock> SemanticBlocks { get; set; }
    public DbSet<Triple> Triples { get; set; }
    public DbSet<TripleProvenance> TripleProvenances { get; set; }
    public DbSet<EntityType> EntityTypes { get; set; }
    public DbSet<GraphEntity> GraphEntities { get; set; }
    public DbSet<GraphRelationship> GraphRelationships { get; set; }
    public DbSet<Notification> Notifications { get; set; }
}