using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Configurations;

public class CardConfig : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasMany(e => e.Talents)
    .WithMany()
    .UsingEntity<Dictionary<string, object>>(
        r => r.HasOne<Talent>()
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict),
        l => l.HasOne<Card>()
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade));

        builder.HasMany(e => e.FabClasses)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                r => r.HasOne<FabClass>()
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict),
                l => l.HasOne<Card>()
                    .WithMany()
                    .OnDelete(DeleteBehavior.Cascade));

        builder.HasMany(e => e.CardSubTypes)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                r => r.HasOne<CardSubType>()
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict),
                l => l.HasOne<Card>()
                    .WithMany()
                    .OnDelete(DeleteBehavior.Cascade));

        builder.HasMany(e => e.CardTypes)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                r => r.HasOne<CardType>()
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict),
                l => l.HasOne<Card>()
                    .WithMany()
                    .OnDelete(DeleteBehavior.Cascade));
    }
}
