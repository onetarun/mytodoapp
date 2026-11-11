using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.DTOs.Tasks;

public sealed record UpdateTaskRequest(
    string Title,
    string? Description = null,
    DateTime? DueDate = null,
    Priority Priority = Priority.Medium,
    int? CategoryId = null) : ITaskWriteRequest;
