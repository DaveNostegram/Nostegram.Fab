using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Configurations;

public class TalentConfig : IEntityTypeConfiguration<Talent>
{
    public void Configure(EntityTypeBuilder<Talent> builder)
    {
        builder.Property(a => a.Name)
            .HasMaxLength(150)
            .IsRequired()
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");
        builder.HasIndex(a => a.Name).IsUnique();
    }
}
