namespace TodoApp.Api.Data.Entities;

public class TodoTask : ISoftDeletable
{
    public const int TitleMaxLength = 200;

    public const int DescriptionMaxLength = 2000;

    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public AppUser? User { get; set; }

    public Category? Category { get; set; }

    public ICollection<SubTask> SubTasks { get; set; } = [];
}
