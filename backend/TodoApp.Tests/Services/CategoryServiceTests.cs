using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Categories;
using TodoApp.Api.Services;
using TodoApp.Tests.Data;

namespace TodoApp.Tests.Services;

public sealed class CategoryServiceTests
{
    private const string UserA = "user-a";
    private const string UserB = "user-b";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Create_ThenList_ReturnsOwnersCategoriesOrderedByName_WithTaskCounts()
    {
        var home = await CreateAsync(UserA, "  Home  ");
        await CreateAsync(UserA, "Errands");
        await SeedTaskAsync(UserA, "Fix tap", home.Id);
        await SeedTaskAsync(UserA, "Old task", home.Id, isDeleted: true);

        home.Name.Should().Be("Home");
        var categories = await WithServiceAsync(UserA, s => s.GetAllAsync());

        categories.Select(c => c.Name).Should().Equal("Errands", "Home");
        categories.Single(c => c.Name == "Home").TaskCount.Should().Be(1);
        categories.Single(c => c.Name == "Errands").TaskCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_DoesNotReturnOtherUsersOrDeletedCategories()
    {
        await CreateAsync(UserA, "Work");
        var deleted = await CreateAsync(UserB, "Gone");
        await CreateAsync(UserB, "Personal");
        await WithServiceAsync(UserB, s => s.DeleteAsync(deleted.Id));

        var categories = await WithServiceAsync(UserB, s => s.GetAllAsync());

        categories.Select(c => c.Name).Should().Equal("Personal");
    }

    [Fact]
    public async Task Create_DuplicateNameForSameUser_IsRejectedCaseInsensitively()
    {
        await CreateAsync(UserA, "Work");

        var result = await WithServiceAsync(UserA, s => s.CreateAsync(new CreateCategoryRequest(" work ")));

        result.Status.Should().Be(ServiceResultStatus.ValidationFailed);
        result.Errors[CategoryService.NameField].Should().ContainSingle().Which.Should().Be(CategoryService.DuplicateNameMessage);
    }

    [Fact]
    public async Task Create_SameNameForDifferentUser_IsAllowed()
    {
        await CreateAsync(UserA, "Work");

        var result = await WithServiceAsync(UserB, s => s.CreateAsync(new CreateCategoryRequest("Work")));

        result.Status.Should().Be(ServiceResultStatus.Success);
    }

    [Fact]
    public async Task Create_NameOfDeletedCategory_IsAllowed()
    {
        var old = await CreateAsync(UserA, "Work");
        await WithServiceAsync(UserA, s => s.DeleteAsync(old.Id));

        var result = await WithServiceAsync(UserA, s => s.CreateAsync(new CreateCategoryRequest("Work")));

        result.Status.Should().Be(ServiceResultStatus.Success);
    }

    [Fact]
    public async Task Create_SetsUserIdFromCurrentUser()
    {
        var created = await CreateAsync(UserA, "Work");

        await using var context = CreateContext(null);
        var stored = await context.Categories.IgnoreQueryFilters().SingleAsync(c => c.Id == created.Id);
        stored.UserId.Should().Be(UserA);
    }

    [Fact]
    public async Task Update_RenamesOwnersCategory()
    {
        var category = await CreateAsync(UserA, "Work");
        await SeedTaskAsync(UserA, "Report", category.Id);

        var result = await WithServiceAsync(UserA, s => s.UpdateAsync(category.Id, new UpdateCategoryRequest(" Office ")));

        result.Status.Should().Be(ServiceResultStatus.Success);
        result.Value.Should().Be(new CategoryDto(category.Id, "Office", 1));
    }

    [Fact]
    public async Task Update_ToSameNameWithDifferentCase_IsAllowed()
    {
        var category = await CreateAsync(UserA, "work");

        var result = await WithServiceAsync(UserA, s => s.UpdateAsync(category.Id, new UpdateCategoryRequest("Work")));

        result.Status.Should().Be(ServiceResultStatus.Success);
        result.Value!.Name.Should().Be("Work");
    }

    [Fact]
    public async Task Update_ToAnotherExistingName_IsRejected()
    {
        await CreateAsync(UserA, "Work");
        var personal = await CreateAsync(UserA, "Personal");

        var result = await WithServiceAsync(UserA, s => s.UpdateAsync(personal.Id, new UpdateCategoryRequest("WORK")));

        result.Status.Should().Be(ServiceResultStatus.ValidationFailed);
    }

    [Fact]
    public async Task Update_UnknownId_ReturnsNotFound()
    {
        var result = await WithServiceAsync(UserA, s => s.UpdateAsync(999, new UpdateCategoryRequest("Any")));

        result.Status.Should().Be(ServiceResultStatus.NotFound);
    }

    [Fact]
    public async Task Delete_SoftDeletesCategory_AndMovesItsTasksToUncategorized()
    {
        var category = await CreateAsync(UserA, "Work");
        var taskId = await SeedTaskAsync(UserA, "Report", category.Id);

        var deleted = await WithServiceAsync(UserA, s => s.DeleteAsync(category.Id));

        deleted.Should().BeTrue();
        await using var context = CreateContext(UserA);
        var stored = await context.Categories.IgnoreQueryFilters().SingleAsync(c => c.Id == category.Id);
        stored.IsDeleted.Should().BeTrue();
        stored.DeletedAt.Should().NotBeNull();
        var task = await context.TodoTasks.SingleAsync(t => t.Id == taskId);
        task.CategoryId.Should().BeNull();
        task.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsFalse()
    {
        (await WithServiceAsync(UserA, s => s.DeleteAsync(999))).Should().BeFalse();
    }

    [Fact]
    public async Task UserB_GetsNotFound_ForUserACategory_OnUpdateAndDelete()
    {
        var category = await CreateAsync(UserA, "Work");
        var taskId = await SeedTaskAsync(UserA, "Report", category.Id);

        var update = await WithServiceAsync(UserB, s => s.UpdateAsync(category.Id, new UpdateCategoryRequest("Hacked")));
        var delete = await WithServiceAsync(UserB, s => s.DeleteAsync(category.Id));

        update.Status.Should().Be(ServiceResultStatus.NotFound);
        delete.Should().BeFalse();
        await using var context = CreateContext(UserA);
        var stored = await context.Categories.SingleAsync(c => c.Id == category.Id);
        stored.Name.Should().Be("Work");
        (await context.TodoTasks.SingleAsync(t => t.Id == taskId)).CategoryId.Should().Be(category.Id);
    }

    private async Task<CategoryDto> CreateAsync(string userId, string name)
    {
        var result = await WithServiceAsync(userId, s => s.CreateAsync(new CreateCategoryRequest(name)));
        result.Status.Should().Be(ServiceResultStatus.Success);
        return result.Value!;
    }

    private async Task<int> SeedTaskAsync(string userId, string title, int categoryId, bool isDeleted = false)
    {
        await using var context = CreateContext(userId);
        var task = new TodoTask { UserId = userId, Title = title, CategoryId = categoryId, IsDeleted = isDeleted };
        context.TodoTasks.Add(task);
        await context.SaveChangesAsync();
        return task.Id;
    }

    private async Task<T> WithServiceAsync<T>(string userId, Func<CategoryService, Task<T>> action)
    {
        var currentUser = new FakeCurrentUserService(userId);
        await using var context = CreateContextFor(currentUser);
        return await action(new CategoryService(context, currentUser));
    }

    private AppDbContext CreateContext(string? userId) => CreateContextFor(new FakeCurrentUserService(userId));

    private AppDbContext CreateContextFor(FakeCurrentUserService currentUser)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new AppDbContext(options, currentUser);
    }
}
