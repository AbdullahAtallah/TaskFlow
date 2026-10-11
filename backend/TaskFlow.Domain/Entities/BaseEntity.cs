namespace TaskFlow.Domain.Entities;

/// <summary>
/// Base class of every soft-deletable entity: the DbContext turns a delete into
/// <see cref="DateTimeDeleted"/> = UtcNow and a global query filter hides deleted rows.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime DateTimeCreated { get; set; }
    public DateTime? DateTimeModified { get; set; }
    public DateTime? DateTimeDeleted { get; set; }

    /// <summary>
    /// Changed by SQL Server on every save of the row. An edit form sends back the version it showed, so a save over
    /// someone else's newer change is refused instead of silently overwriting it.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
