using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Configurations;

public class FabClassConfig : IEntityTypeConfiguration<FabClass>
{
    public void Configure(EntityTypeBuilder<FabClass> builder)
    {
    }
}
