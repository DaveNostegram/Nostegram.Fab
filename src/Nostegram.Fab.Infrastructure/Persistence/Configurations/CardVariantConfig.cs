using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Configurations;

public class CardVariantConfig : IEntityTypeConfiguration<CardVariant>
{
    public void Configure(EntityTypeBuilder<CardVariant> builder)
    {

    }
}
