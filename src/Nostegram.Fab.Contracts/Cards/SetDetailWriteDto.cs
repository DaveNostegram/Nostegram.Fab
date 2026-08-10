namespace Nostegram.Fab.Contracts.Cards;

public class SetDetailWriteDto
{
    public Guid? PublicId { get; init; }
    public required RarityEnumAPI Rarity { get; set; }
    public required Guid SetId { get; set; }
    public required string CollectorNumber { get; set; }
    public required Guid ArtistId { get; set; }
}