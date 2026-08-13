namespace Nostegram.Fab.Domain;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public Guid PublicId { get; protected set; } = Guid.NewGuid();
}