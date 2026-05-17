namespace Nostegram.Fab.Application.Exceptions;

public class ConflictException(string name, string usedBy) : Exception($"'{name}' is used by a '{usedBy}'.")
{
    public string Name { get; } = name;
    public string UsedBy { get; } = usedBy;
}
