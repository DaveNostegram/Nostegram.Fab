using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Configurations;

public class CardConfig : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasMany(e => e.Talents).WithMany();
        builder.HasMany(e => e.FabClasses).WithMany();
        builder.HasMany(e => e.CardSubTypes).WithMany();
        builder.HasMany(e => e.CardTypes).WithMany();
    }
}
