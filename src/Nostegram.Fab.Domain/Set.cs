namespace Nostegram.Fab.Domain;

public class Set : BaseEntity
{
    public required string Name { get; set; }
    public required string SetCode { get; set; }
    public required DateOnly ReleaseDate { get; set; }
}