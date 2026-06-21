namespace Nostegram.Fab.Contracts.Sets;

public class SetWriteDto(string name, string setCode, DateOnly releaseDate)
{
    public string Name { get; init; } = name;
    public string SetCode { get; init; } = setCode;
    public DateOnly ReleaseDate { get; init; } = releaseDate;
}