namespace Nostegram.Fab.Application.Exceptions;

public class AlreadyExistsException(string type, string name) : Exception($"{type} '{name}' already exists.")
{
    public string Name { get; } = name;
    public string Type { get; } = type;
}
