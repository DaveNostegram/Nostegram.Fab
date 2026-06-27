namespace Nostegram.Fab.Application.Exceptions;

public class AlreadyExistsException : Exception
{
    public IReadOnlyDictionary<string, string> Conflicts { get; }

    public string Type { get; }
    public string Name { get; }

    public AlreadyExistsException(string type, string name)
        : base($"{type} '{name}' already exists.")
    {
        Type = type;
        Name = name;
        Conflicts = new Dictionary<string, string>
        {
            [type] = name
        };
    }

    public AlreadyExistsException(IDictionary<string, string> conflicts)
        : base(CreateMessage(conflicts))
    {
        Conflicts = new Dictionary<string, string>(conflicts);

        var first = conflicts.First();
        Type = first.Key;
        Name = first.Value;
    }

    private static string CreateMessage(IDictionary<string, string> conflicts)
    {
        var items = conflicts.Select(c => $"{c.Key} '{c.Value}'");

        return $"{string.Join(" and ", items)} already {(conflicts.Count == 1 ? "exists" : "exist")}.";
    }
}