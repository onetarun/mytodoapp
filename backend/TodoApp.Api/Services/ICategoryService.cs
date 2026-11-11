using TodoApp.Api.DTOs.Categories;

namespace TodoApp.Api.Services;

/// <summary>
/// Categories of the current user (FR-15, FR-16). The user always comes from the JWT.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// The current user's categories ordered by name, each with its count of non-deleted tasks.
    /// </summary>
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a category. A name already used by this user (case-insensitive) fails validation.
    /// </summary>
    Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a category. Not found when it does not exist or belongs to another user.
    /// </summary>
    Task<ServiceResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a category and moves its tasks to Uncategorized in one save.
    /// Returns <c>false</c> when it does not exist or belongs to another user.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
