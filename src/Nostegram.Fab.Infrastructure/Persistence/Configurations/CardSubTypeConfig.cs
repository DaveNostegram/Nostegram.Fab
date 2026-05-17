using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Configurations;

public class CardSubTypeConfig : IEntityTypeConfiguration<CardSubType>
{
    public void Configure(EntityTypeBuilder<CardSubType> builder)
    {

    }
}
