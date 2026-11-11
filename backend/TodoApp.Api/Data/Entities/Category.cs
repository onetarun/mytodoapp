namespace TodoApp.Api.Data.Entities;

public class Category : ISoftDeletable
{
    public const int NameMaxLength = 50;

    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public AppUser? User { get; set; }

    public ICollection<TodoTask> Tasks { get; set; } = [];
}
