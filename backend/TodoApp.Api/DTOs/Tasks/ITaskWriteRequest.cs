using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.DTOs.Tasks;

/// <summary>
/// Editable task fields shared by create and update, so both use the same validation rules.
/// </summary>
public interface ITaskWriteRequest
{
    string Title { get; }

    string? Description { get; }

    DateTime? DueDate { get; }

    Priority Priority { get; }

    int? CategoryId { get; }
}
