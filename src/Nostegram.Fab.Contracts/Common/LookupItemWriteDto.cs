using Nostegram.Fab.Contracts.Normalisations;

namespace Nostegram.Fab.Contracts.Common;

public sealed class LookupItemWriteDto(string name)
{
    public string Name { get; } = NameNormaliser.ForDisplay(name);
}