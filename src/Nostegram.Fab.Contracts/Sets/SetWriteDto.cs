using Nostegram.Fab.Contracts.Normalisations;

namespace Nostegram.Fab.Contracts.Sets;

public sealed class SetWriteDto(
    string name,
    string setCode,
    DateOnly releaseDate)
{
    public string Name { get; } =
        NameNormaliser.ForDisplay(name);

    public string SetCode { get; } =
        NameNormaliser.ForDisplay(setCode);

    public DateOnly ReleaseDate { get; } =
        releaseDate;
}