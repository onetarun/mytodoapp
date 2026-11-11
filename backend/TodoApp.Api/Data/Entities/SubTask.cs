namespace TodoApp.Api.Data.Entities;

public class SubTask : ISoftDeletable
{
    public const int TitleMaxLength = 200;

    public int Id { get; set; }

    public int TodoTaskId { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public int SortOrder { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public TodoTask? TodoTask { get; set; }
}
