using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Sets;

namespace Nostegram.Fab.Contracts.Cards;

public class SetDetailDto
{
    public required Guid PublicId { get; init; }
    public RarityEnumAPI Rarity { get; set; }
    public required SetDto Set { get; set; }
    public required string CollectorNumber { get; set; }
    public required LookupItemDto Artist { get; set; }
}