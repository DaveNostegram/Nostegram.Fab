namespace Nostegram.Fab.Domain;

public class CardVariant : BaseEntity
{
    public string? CardText { get; set; }
    public int? Cost { get; set; }
    public int? Block { get; set; }
    public int? Attack { get; set; }
    public int? Health { get; set; }
    public int? Intellect { get; set; }
    public PitchEnum? Pitch { get; set; }
    public virtual List<SetDetail> SetDetails { get; set; } = [];
    public int CardId { get; set; }
    public virtual Card Card { get; set; } = null!;
}