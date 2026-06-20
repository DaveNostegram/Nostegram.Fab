using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Configurations;

public class CardTypeConfig : IEntityTypeConfiguration<CardType>
{
    public void Configure(EntityTypeBuilder<CardType> builder)
    {
        builder.Property(a => a.Name)
            .HasMaxLength(150)
            .IsRequired()
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");
        builder.HasIndex(a => a.Name).IsUnique();
    }
}
