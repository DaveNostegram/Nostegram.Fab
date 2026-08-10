namespace Nostegram.Fab.Contracts.Cards;

public class CardVariantDto
{
    public required Guid PublicId { get; init; }
    public string? CardText { get; set; }
    public int? Cost { get; set; }
    public int? Block { get; set; }
    public int? Power { get; set; }
    public int? Health { get; set; }
    public int? Intellect { get; set; }
    public PitchEnumAPI Pitch { get; set; }
    public List<SetDetailDto> SetDetails { get; set; } = [];
}