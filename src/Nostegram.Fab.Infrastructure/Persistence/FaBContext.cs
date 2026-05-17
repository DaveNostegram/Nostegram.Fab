using Humanizer;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Application.Common.Interfaces;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence;

public class FabDbContext(DbContextOptions options) : DbContext(options), ICommit
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var currentTableName = entity.GetTableName();
            entity.SetTableName(currentTableName.Pluralize());
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FabDbContext).Assembly);
        modelBuilder.HasDefaultSchema("dbo");
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Artist> Artists { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardSubType> CardSubTypes { get; set; }
    public DbSet<CardType> CardTypes { get; set; }
    public DbSet<CardVariant> CardVariants { get; set; }
    public DbSet<FabClass> FabClasses { get; set; }
    public DbSet<Set> Sets { get; set; }
    public DbSet<SetDetail> SetDetails { get; set; }
    public DbSet<Talent> Talents { get; set; }
}
