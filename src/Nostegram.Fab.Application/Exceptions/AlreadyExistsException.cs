namespace Nostegram.Fab.Application.Exceptions;

public class AlreadyExistsException(string name) : Exception($"'{name}' already exists.")
{
    public string Name { get; } = name;
}
