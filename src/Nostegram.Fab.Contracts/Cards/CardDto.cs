using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Normalisations;

namespace Nostegram.Fab.Contracts.Cards;

public class CardDto
{
    public required Guid PublicId { get; init; }
    private string _name = null!;
    public required string Name
    {
        get => _name;
        set => _name = NameNormaliser.ForDisplay(value);
    }
    public List<CardVariantDto> CardVariants { get; set; } = [];
    public List<LookupItemDto> Talents { get; set; } = [];
    public List<LookupItemDto> CardTypes { get; set; } = [];
    public List<LookupItemDto> CardSubTypes { get; set; } = [];
    public List<LookupItemDto> FabClasses { get; set; } = [];
    public LookupItemDto? FlipCard { get; set; }
}