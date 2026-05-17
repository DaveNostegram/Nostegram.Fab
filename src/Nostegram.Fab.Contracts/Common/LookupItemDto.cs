namespace Nostegram.Fab.Contracts.Common;

public class LookupItemDto(Guid publicId, string name)
{
    public Guid PublicId { get; init; } = publicId;
    public string Name { get; init; } = name;
}