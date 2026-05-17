namespace Nostegram.Fab.Application.Exceptions;

public class NotFoundException(string name) : Exception($"'{name}' not found.")
{
    public string Name { get; } = name;
}
