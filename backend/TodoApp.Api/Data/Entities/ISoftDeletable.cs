namespace TodoApp.Api.Data.Entities;

/// <summary>
/// Entities that are soft deleted (NFR-7): a delete sets <see cref="IsDeleted"/> and <see cref="DeletedAt"/>
/// instead of removing the row.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTime? DeletedAt { get; set; }
}
