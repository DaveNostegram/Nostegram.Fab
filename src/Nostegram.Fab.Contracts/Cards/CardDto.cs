using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Contracts.Cards;

public class CardDto
{
    public required Guid PublicId { get; init; }
    public required string Name { get; set; }
    public List<CardVariantDto> CardVariants { get; set; } = [];
    public List<LookupItemDto> Talents { get; set; } = [];
    public List<LookupItemDto> CardTypes { get; set; } = [];
    public List<LookupItemDto> CardSubTypes { get; set; } = [];
    public Guid? FlipCard { get; set; }
}