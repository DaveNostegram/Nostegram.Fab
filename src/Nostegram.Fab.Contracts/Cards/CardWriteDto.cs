using Nostegram.Fab.Contracts.Normalisations;

namespace Nostegram.Fab.Contracts.Cards;

public class CardWriteDto
{
    private string _name = null!;
    public required string Name
    {
        get => _name;
        set => _name = NameNormaliser.ForDisplay(value);
    }
    public List<CardVariantWriteDto> CardVariants { get; set; } = [];
    public List<Guid> Talents { get; set; } = [];
    public List<Guid> CardTypes { get; set; } = [];
    public List<Guid> CardSubTypes { get; set; } = [];
    public List<Guid> FabClasses { get; set; } = [];
    public Guid? FlipCard { get; set; }
}