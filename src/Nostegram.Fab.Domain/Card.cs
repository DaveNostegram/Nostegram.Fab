namespace Nostegram.Fab.Domain;

public class Card : BaseEntity
{
    public required string Name { get; set; }
    public virtual ICollection<Talent> Talents { get; set; } = [];
    public virtual ICollection<FabClass> FabClasses { get; set; } = [];
    public virtual List<CardVariant> CardVariants { get; set; } = [];
    public virtual ICollection<CardType> CardTypes { get; set; } = [];
    public virtual ICollection<CardSubType> CardSubTypes { get; set; } = [];
    public int? FlipCardId { get; set; }
    public virtual Card? FlipCard { get; set; }
}
