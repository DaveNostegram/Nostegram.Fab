namespace Nostegram.Fab.Application.Exceptions;

public class RequiredFieldException : Exception
{
    public string Field { get; }
    public IReadOnlySet<string> Conflicts { get; }

    public RequiredFieldException(string field)
        : base($"'{field}' is required.")
    {
        Field = field;
        Conflicts = new HashSet<string> { field };
    }

    public RequiredFieldException(IEnumerable<string> conflicts)
        : base(CreateMessage(conflicts))
    {
        Conflicts = new HashSet<string>(conflicts);

        var first = conflicts.First();
        Field = first;
    }

    private static string CreateMessage(IEnumerable<string> conflicts)
    {
        var fields = conflicts.ToArray();

        return $"{string.Join(" and ", fields.Select(c => $"'{c}'"))} {(fields.Length == 1 ? "is required" : "are required")}.";
    }
}
