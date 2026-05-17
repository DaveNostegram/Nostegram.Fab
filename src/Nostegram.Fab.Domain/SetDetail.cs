namespace Nostegram.Fab.Domain;

public class SetDetail : BaseEntity
{
    public RarityEnum Rarity { get; set; }
    public int SetId { get; set; }
    public virtual Set Set { get; set; } = null!;
    public required string CollectorNumber { get; set; }
    public int ArtistId { get; set; }
    public virtual Artist Artist { get; set; } = null!;
    public int CardVariantId { get; set; }
    public virtual CardVariant CardVariant { get; set; } = null!;
}