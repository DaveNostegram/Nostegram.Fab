namespace Nostegram.Fab.Contracts.Cards;

public class CardWriteDto
{
    public required string Name { get; set; }
    public List<CardVariantWriteDto> CardVariants { get; set; } = [];
    public List<Guid> Talents { get; set; } = [];
    public List<Guid> CardTypes { get; set; } = [];
    public List<Guid> CardSubTypes { get; set; } = [];
    public List<Guid> FabClasses { get; set; } = [];
    public Guid? FlipCard { get; set; }
}