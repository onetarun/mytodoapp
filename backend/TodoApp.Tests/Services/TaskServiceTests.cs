using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Categories;
using TodoApp.Api.DTOs.Tasks;
using TodoApp.Api.Services;
using TodoApp.Tests.Data;

namespace TodoApp.Tests.Services;

public sealed class TaskServiceTests
{
    private const string UserA = "user-a";
    private const string UserB = "user-b";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Create_WithOnlyTitle_DefaultsToMediumAndNotCompleted_ForCurrentUser()
    {
        var created = await WithServiceAsync(UserA, s => s.CreateAsync(new CreateTaskRequest("  Buy milk  ")));

        created.Title.Should().Be("Buy milk");
        created.Priority.Should().Be(Priority.Medium);
        created.IsCompleted.Should().BeFalse();
        created.CompletedAt.Should().BeNull();
        created.CategoryId.Should().BeNull();
        created.IsOverdue.Should().BeFalse();
        created.SubTasks.Should().BeEmpty();
        created.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);

        await using var context = CreateContext(null);
        (await context.TodoTasks.IgnoreQueryFilters().SingleAsync(t => t.Id == created.Id)).UserId.Should().Be(UserA);
    }

    [Fact]
    public async Task Create_WithAllFields_StoresThemAndReturnsCategoryName()
    {
        var categoryId = await SeedCategoryAsync(UserA, "Work");
        var due = DateTime.UtcNow.AddDays(2);

        var created = await WithServiceAsync(UserA, s =>
            s.CreateAsync(new CreateTaskRequest("Report", "Quarterly numbers", due, Priority.High, categoryId)));

        created.Description.Should().Be("Quarterly numbers");
        created.DueDate.Should().Be(due);
        created.Priority.Should().Be(Priority.High);
        created.CategoryId.Should().Be(categoryId);
        created.CategoryName.Should().Be("Work");
    }

    [Fact]
    public async Task GetAll_FiltersByStatus_AndOrdersPendingFirstThenDueDateThenNewest()
    {
        var now = DateTime.UtcNow;
        var noDue = await CreateAsync(UserA, "No due date");
        var later = await CreateAsync(UserA, "Due later", now.AddDays(3));
        var soon = await CreateAsync(UserA, "Due soon", now.AddDays(1));
        var done = await CreateAsync(UserA, "Done", now.AddDays(-1));
        await WithServiceAsync(UserA, s => s.SetCompletionAsync(done, true));

        var all = await WithServiceAsync(UserA, s => s.GetAllAsync(TaskStatusFilter.All));
        var pending = await WithServiceAsync(UserA, s => s.GetAllAsync(TaskStatusFilter.Pending));
        var completed = await WithServiceAsync(UserA, s => s.GetAllAsync(TaskStatusFilter.Completed));

        all.Select(t => t.Id).Should().Equal(soon, later, noDue, done);
        pending.Select(t => t.Id).Should().Equal(soon, later, noDue);
        completed.Select(t => t.Id).Should().Equal(done);
    }

    [Fact]
    public async Task GetAll_TasksWithoutDueDate_AreOrderedNewestFirst()
    {
        var first = await CreateAsync(UserA, "First");
        var second = await CreateAsync(UserA, "Second");

        var all = await WithServiceAsync(UserA, s => s.GetAllAsync(TaskStatusFilter.All));

        all.Select(t => t.Id).Should().Equal(second, first);
    }

    [Fact]
    public async Task SetCompletion_SetsCompletedAt_AndReopenClearsIt()
    {
        var id = await CreateAsync(UserA, "Task");

        var completed = await WithServiceAsync(UserA, s => s.SetCompletionAsync(id, true));
        completed!.IsCompleted.Should().BeTrue();
        completed.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        completed.CompletedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);

        var reopened = await WithServiceAsync(UserA, s => s.SetCompletionAsync(id, false));
        reopened!.IsCompleted.Should().BeFalse();
        reopened.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task SetCompletion_AlreadyCompleted_KeepsOriginalCompletedAt()
    {
        var id = await CreateAsync(UserA, "Task");
        var first = await WithServiceAsync(UserA, s => s.SetCompletionAsync(id, true));

        var second = await WithServiceAsync(UserA, s => s.SetCompletionAsync(id, true));

        second!.CompletedAt.Should().Be(first!.CompletedAt);
    }

    [Fact]
    public async Task IsOverdue_IsTrueOnlyForPastDueNotCompletedTasks()
    {
        var now = DateTime.UtcNow;
        var pastPending = await CreateAsync(UserA, "Past pending", now.AddHours(-1));
        var pastDone = await CreateAsync(UserA, "Past done", now.AddHours(-1));
        var future = await CreateAsync(UserA, "Future", now.AddHours(1));
        var noDue = await CreateAsync(UserA, "No due");
        await WithServiceAsync(UserA, s => s.SetCompletionAsync(pastDone, true));

        var all = await WithServiceAsync(UserA, s => s.GetAllAsync(TaskStatusFilter.All));
        var detail = await WithServiceAsync(UserA, s => s.GetByIdAsync(pastPending));

        all.Where(t => t.IsOverdue).Select(t => t.Id).Should().Equal(pastPending);
        all.Select(t => t.Id).Should().Contain([future, noDue, pastDone]);
        detail!.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public async Task Update_ChangesAllEditableFields_AndUpdatedAt()
    {
        var categoryId = await SeedCategoryAsync(UserA, "Home");
        var id = await CreateAsync(UserA, "Old");
        var before = await WithServiceAsync(UserA, s => s.GetByIdAsync(id));
        var due = DateTime.UtcNow.AddDays(5);

        var updated = await WithServiceAsync(UserA, s =>
            s.UpdateAsync(id, new UpdateTaskRequest("New", "Details", due, Priority.Low, categoryId)));

        updated.Should().NotBeNull();
        updated!.Title.Should().Be("New");
        updated.Description.Should().Be("Details");
        updated.DueDate.Should().Be(due);
        updated.Priority.Should().Be(Priority.Low);
        updated.CategoryName.Should().Be("Home");
        updated.UpdatedAt.Should().BeOnOrAfter(before!.UpdatedAt);
        updated.CreatedAt.Should().Be(before.CreatedAt);
    }

    [Fact]
    public async Task UnknownId_IsNotFound_ForEveryOperation()
    {
        (await WithServiceAsync(UserA, s => s.GetByIdAsync(999))).Should().BeNull();
        (await WithServiceAsync(UserA, s => s.UpdateAsync(999, new UpdateTaskRequest("X")))).Should().BeNull();
        (await WithServiceAsync(UserA, s => s.SetCompletionAsync(999, true))).Should().BeNull();
        (await WithServiceAsync(UserA, s => s.DeleteAsync(999))).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_SoftDeletes_AndTaskIsHiddenFromListAndDetail()
    {
        var id = await CreateAsync(UserA, "To delete");
        await CreateAsync(UserA, "Keep");

        (await WithServiceAsync(UserA, s => s.DeleteAsync(id))).Should().BeTrue();

        (await WithServiceAsync(UserA, s => s.GetAllAsync(TaskStatusFilter.All))).Select(t => t.Title).Should().Equal("Keep");
        (await WithServiceAsync(UserA, s => s.GetByIdAsync(id))).Should().BeNull();
        (await WithServiceAsync(UserA, s => s.DeleteAsync(id))).Should().BeFalse();

        await using var context = CreateContext(UserA);
        var stored = await context.TodoTasks.IgnoreQueryFilters().SingleAsync(t => t.Id == id);
        stored.IsDeleted.Should().BeTrue();
        stored.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UserB_GetsNotFound_ForUserATask_OnGetUpdateCompleteAndDelete()
    {
        var id = await CreateAsync(UserA, "Private");

        (await WithServiceAsync(UserB, s => s.GetByIdAsync(id))).Should().BeNull();
        (await WithServiceAsync(UserB, s => s.UpdateAsync(id, new UpdateTaskRequest("Hacked")))).Should().BeNull();
        (await WithServiceAsync(UserB, s => s.SetCompletionAsync(id, true))).Should().BeNull();
        (await WithServiceAsync(UserB, s => s.DeleteAsync(id))).Should().BeFalse();
        (await WithServiceAsync(UserB, s => s.GetAllAsync(TaskStatusFilter.All))).Should().BeEmpty();

        var stillThere = await WithServiceAsync(UserA, s => s.GetByIdAsync(id));
        stillThere!.Title.Should().Be("Private");
        stillThere.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task AfterCategoryDelete_TasksShowNoCategory()
    {
        var categoryId = await SeedCategoryAsync(UserA, "Work");
        var id = await WithServiceAsync(UserA, async s =>
            (await s.CreateAsync(new CreateTaskRequest("Report", CategoryId: categoryId))).Id);

        await using (var context = CreateContext(UserA))
        {
            (await new CategoryService(context, new FakeCurrentUserService(UserA)).DeleteAsync(categoryId)).Should().BeTrue();
        }

        var detail = await WithServiceAsync(UserA, s => s.GetByIdAsync(id));
        detail!.CategoryId.Should().BeNull();
        detail.CategoryName.Should().BeNull();
        (await WithServiceAsync(UserA, s => s.GetAllAsync(TaskStatusFilter.All))).Single().CategoryId.Should().BeNull();
    }

    private async Task<int> CreateAsync(string userId, string title, DateTime? dueDate = null) =>
        await WithServiceAsync(userId, async s => (await s.CreateAsync(new CreateTaskRequest(title, DueDate: dueDate))).Id);

    private async Task<int> SeedCategoryAsync(string userId, string name)
    {
        await using var context = CreateContext(userId);
        var result = await new CategoryService(context, new FakeCurrentUserService(userId))
            .CreateAsync(new CreateCategoryRequest(name));
        return result.Value!.Id;
    }

    private async Task<T> WithServiceAsync<T>(string userId, Func<TaskService, Task<T>> action)
    {
        var currentUser = new FakeCurrentUserService(userId);
        await using var context = CreateContextFor(currentUser);
        return await action(new TaskService(context, currentUser));
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
