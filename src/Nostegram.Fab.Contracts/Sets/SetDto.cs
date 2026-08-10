namespace Nostegram.Fab.Contracts.Sets;

public class SetDto(Guid publicId, string name, string setCode, DateOnly releaseDate)
{
    public Guid PublicId { get; init; } = publicId;
    public string Name { get; init; } = name;
    public string SetCode { get; init; } = setCode;
    public DateOnly ReleaseDate { get; init; } = releaseDate;
}