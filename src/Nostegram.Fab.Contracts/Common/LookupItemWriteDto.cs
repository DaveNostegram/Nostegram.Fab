namespace Nostegram.Fab.Contracts.Common;

public class LookupItemWriteDto(string name)
{
    public string Name { get; init; } = name;
}