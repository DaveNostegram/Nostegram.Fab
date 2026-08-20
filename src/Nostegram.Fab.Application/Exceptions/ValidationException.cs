namespace Nostegram.Fab.Application.Exceptions;

public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, List<string>> Errors { get; }

    public ValidationException(
        IReadOnlyDictionary<string, List<string>> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}