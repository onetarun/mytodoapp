namespace TodoApp.Api.DTOs.Categories;

/// <summary>
/// A category with the number of its non-deleted tasks.
/// </summary>
public sealed record CategoryDto(int Id, string Name, int TaskCount);
