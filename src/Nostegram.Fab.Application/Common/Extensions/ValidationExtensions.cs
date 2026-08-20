namespace Nostegram.Fab.Application.Common.Extensions;

public static class ValidationExtensions
{
    public static void AddValidationError(
        this Dictionary<string, List<string>> validationErrors,
        string key,
        string error)
    {
        if (!validationErrors.TryGetValue(key, out var errors))
        {
            errors = [];
            validationErrors[key] = errors;
        }

        errors.Add(error);
    }
}