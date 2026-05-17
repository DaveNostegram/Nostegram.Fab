namespace Nostegram.Fab.Application.Exceptions;

public class RequiredFieldException(string field) : Exception($"'{field}' is required.")
{
    public string Field { get; } = field;
}
