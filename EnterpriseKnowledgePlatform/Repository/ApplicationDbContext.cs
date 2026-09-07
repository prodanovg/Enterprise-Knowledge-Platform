    using Domain.Models;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    namespace Repository;

    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<User>(options)
    {
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
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApiKey>()
                .HasOne(x => x.User)
                .WithMany(x => x.ApiKeys)
                .HasForeignKey(x => x.UserId);

            builder.Entity<Document>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.OwnerId);

            builder.Entity<Notification>()
                .HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId);

            builder.Entity<ProcessingJob>()
                .HasOne(x => x.Document)
                .WithMany(x => x.ProcessingJobs)
                .HasForeignKey(x => x.DocumentId);

            builder.Entity<SemanticBlock>()
                .HasOne(x => x.Document)
                .WithMany(x => x.SemanticBlocks)
                .HasForeignKey(x => x.DocumentId);

            builder.Entity<TripleProvenance>()
                .HasOne(x => x.Triple)
                .WithMany(x => x.TripleProvenances)
                .HasForeignKey(x => x.TripleId);

            builder.Entity<TripleProvenance>()
                .HasOne(x => x.Document)
                .WithMany(x => x.TripleProvenances)
                .HasForeignKey(x => x.DocumentId);

            builder.Entity<TripleProvenance>()
                .HasOne(x => x.SemanticBlock)
                .WithMany(x => x.TripleProvenances)
                .HasForeignKey(x => x.SemanticBlockId);

            builder.Entity<GraphEntity>()
                .HasOne(x => x.EntityType)
                .WithMany(x => x.GraphEntities)
                .HasForeignKey(x => x.EntityTypeId);

            builder.Entity<GraphRelationship>()
                .HasOne(x => x.SourceEntity)
                .WithMany(x => x.OutgoingRelationships)
                .HasForeignKey(x => x.SourceEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GraphRelationship>()
                .HasOne(x => x.TargetEntity)
                .WithMany(x => x.IncomingRelationships)
                .HasForeignKey(x => x.TargetEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }