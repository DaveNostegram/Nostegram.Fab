using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Infrastructure.Persistence.Configurations;

public class SetDetailConfig : IEntityTypeConfiguration<SetDetail>
{
    public void Configure(EntityTypeBuilder<SetDetail> builder)
    {
        builder.HasOne(e => e.Artist).WithMany().HasForeignKey(e => e.ArtistId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Set).WithMany().HasForeignKey(e => e.SetId).IsRequired().OnDelete(DeleteBehavior.Restrict);
    }
}
