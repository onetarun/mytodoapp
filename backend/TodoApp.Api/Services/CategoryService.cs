using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Categories;

namespace TodoApp.Api.Services;

/// <summary>
/// The ownership + soft-delete query filters on <see cref="AppDbContext"/> limit every query
/// to the current user's non-deleted rows, so another user's category is simply not found.
/// </summary>
public sealed class CategoryService(AppDbContext dbContext, ICurrentUserService currentUserService) : ICategoryService
{
    public const string NameField = "name";

    public const string DuplicateNameMessage = "You already have a category with this name.";

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Tasks.Count(t => !t.IsDeleted)))
            .ToListAsync(cancellationToken);

    public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId
            ?? throw new InvalidOperationException("Creating a category requires an authenticated user.");
        var name = request.Name.Trim();

        if (await NameExistsAsync(name, exceptId: null, cancellationToken))
        {
            return ServiceResult<CategoryDto>.ValidationFailed(NameField, DuplicateNameMessage);
        }

        var category = new Category { UserId = userId, Name = name };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<CategoryDto>.Success(new CategoryDto(category.Id, category.Name, 0));
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceResult<CategoryDto>.NotFound();
        }

        var name = request.Name.Trim();
        if (await NameExistsAsync(name, exceptId: id, cancellationToken))
        {
            return ServiceResult<CategoryDto>.ValidationFailed(NameField, DuplicateNameMessage);
        }

        category.Name = name;
        await dbContext.SaveChangesAsync(cancellationToken);

        var taskCount = await dbContext.TodoTasks.CountAsync(t => t.CategoryId == id, cancellationToken);
        return ServiceResult<CategoryDto>.Success(new CategoryDto(category.Id, category.Name, taskCount));
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        // FR-16: the tasks become Uncategorized and are not deleted.
        var tasks = await dbContext.TodoTasks.Where(t => t.CategoryId == id).ToListAsync(cancellationToken);
        foreach (var task in tasks)
        {
            task.CategoryId = null;
        }

        // Converted to a soft delete by AppDbContext; both changes go in one SaveChanges (one transaction).
        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private Task<bool> NameExistsAsync(string name, int? exceptId, CancellationToken cancellationToken)
    {
        var lowerName = name.ToLower();
        return dbContext.Categories.AnyAsync(
            c => c.Name.ToLower() == lowerName && (exceptId == null || c.Id != exceptId),
            cancellationToken);
    }
}
