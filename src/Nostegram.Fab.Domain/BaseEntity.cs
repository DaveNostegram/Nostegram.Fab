namespace Nostegram.Fab.Domain;

public class BaseEntity
{
    public int Id { get; protected set; }
    public Guid PublicId { get; protected set; } = Guid.NewGuid();
}