using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;

namespace TodoApp.Tests.Data;

public sealed class AppDbContextTests
{
    private const string UserA = "user-a";
    private const string UserB = "user-b";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task RemovedTask_IsNotReturnedByQueries_ButRowIsKeptAsSoftDeleted()
    {
        var taskId = await SeedTaskAsync(UserA, "Task to delete");

        await using (var context = CreateContext(UserA))
        {
            var task = await context.TodoTasks.SingleAsync(t => t.Id == taskId);
            context.TodoTasks.Remove(task);
            await context.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext(UserA);
        (await verifyContext.TodoTasks.AnyAsync(t => t.Id == taskId)).Should().BeFalse();

        var stored = await verifyContext.TodoTasks.IgnoreQueryFilters().SingleAsync(t => t.Id == taskId);
        stored.IsDeleted.Should().BeTrue();
        stored.DeletedAt.Should().NotBeNull();
        stored.DeletedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task RemovedCategory_IsSoftDeleted()
    {
        int categoryId;
        await using (var context = CreateContext(UserA))
        {
            var category = new Category { UserId = UserA, Name = "Work" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            categoryId = category.Id;

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext(UserA);
        (await verifyContext.Categories.AnyAsync()).Should().BeFalse();
        var stored = await verifyContext.Categories.IgnoreQueryFilters().SingleAsync(c => c.Id == categoryId);
        stored.IsDeleted.Should().BeTrue();
        stored.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RemovedTask_SoftDeletesItsLoadedSubTasks()
    {
        var taskId = await SeedTaskAsync(UserA, "Task with checklist", subTaskCount: 2);

        await using (var context = CreateContext(UserA))
        {
            var task = await context.TodoTasks.Include(t => t.SubTasks).SingleAsync(t => t.Id == taskId);
            context.TodoTasks.Remove(task);
            await context.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext(UserA);
        (await verifyContext.SubTasks.AnyAsync()).Should().BeFalse();
        var stored = await verifyContext.SubTasks.IgnoreQueryFilters().ToListAsync();
        stored.Should().HaveCount(2).And.OnlyContain(s => s.IsDeleted && s.DeletedAt != null);
    }

    [Fact]
    public async Task RemovedCategory_ClearsCategoryOnLoadedTasks()
    {
        int taskId;
        await using (var context = CreateContext(UserA))
        {
            var category = new Category { UserId = UserA, Name = "Work" };
            var task = new TodoTask { UserId = UserA, Title = "Report", Category = category };
            context.TodoTasks.Add(task);
            await context.SaveChangesAsync();
            taskId = task.Id;

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext(UserA);
        var stored = await verifyContext.TodoTasks.SingleAsync(t => t.Id == taskId);
        stored.CategoryId.Should().BeNull();
    }

    [Fact]
    public async Task UserA_CannotQueryUserBTasks()
    {
        await SeedTaskAsync(UserA, "A's task");
        var userBTaskId = await SeedTaskAsync(UserB, "B's task");

        await using var context = CreateContext(UserA);
        var tasks = await context.TodoTasks.ToListAsync();

        tasks.Should().ContainSingle().Which.Title.Should().Be("A's task");
        (await context.TodoTasks.SingleOrDefaultAsync(t => t.Id == userBTaskId)).Should().BeNull();
    }

    [Fact]
    public async Task UserA_CannotQueryUserBCategories()
    {
        await using (var seedContext = CreateContext(UserB))
        {
            seedContext.Categories.AddRange(
                new Category { UserId = UserA, Name = "A-Work" },
                new Category { UserId = UserB, Name = "B-Work" });
            await seedContext.SaveChangesAsync();
        }

        await using var context = CreateContext(UserA);
        var categories = await context.Categories.ToListAsync();

        categories.Should().ContainSingle().Which.Name.Should().Be("A-Work");
    }

    [Fact]
    public async Task UnauthenticatedContext_SeesNoTasksOrCategories()
    {
        await SeedTaskAsync(UserA, "A's task");

        await using var context = CreateContext(userId: null);

        (await context.TodoTasks.AnyAsync()).Should().BeFalse();
        (await context.Categories.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task NewTask_GetsUtcCreatedAtAndUpdatedAt_AndDefaultPriorityMedium()
    {
        var before = DateTime.UtcNow;
        var taskId = await SeedTaskAsync(UserA, "New task");

        await using var context = CreateContext(UserA);
        var task = await context.TodoTasks.SingleAsync(t => t.Id == taskId);

        task.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        task.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
        task.UpdatedAt.Should().Be(task.CreatedAt);
        task.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public async Task ModifiedTask_GetsNewUpdatedAt_AndKeepsCreatedAt()
    {
        var taskId = await SeedTaskAsync(UserA, "Original");
        DateTime createdAt;

        await using (var context = CreateContext(UserA))
        {
            var task = await context.TodoTasks.SingleAsync(t => t.Id == taskId);
            createdAt = task.CreatedAt;
            await Task.Delay(TimeSpan.FromMilliseconds(20));
            task.Title = "Changed";
            await context.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext(UserA);
        var stored = await verifyContext.TodoTasks.SingleAsync(t => t.Id == taskId);
        stored.CreatedAt.Should().Be(createdAt);
        stored.UpdatedAt.Should().BeAfter(createdAt);
    }

    [Fact]
    public async Task ExplicitLowPriority_IsKept()
    {
        await using (var context = CreateContext(UserA))
        {
            context.TodoTasks.Add(new TodoTask { UserId = UserA, Title = "Low", Priority = Priority.Low });
            await context.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext(UserA);
        (await verifyContext.TodoTasks.SingleAsync()).Priority.Should().Be(Priority.Low);
    }

    [Fact]
    public async Task LocalDueDate_IsStoredAsUtc()
    {
        var localDueDate = new DateTime(2026, 12, 1, 18, 30, 0, DateTimeKind.Local);

        await using (var context = CreateContext(UserA))
        {
            context.TodoTasks.Add(new TodoTask { UserId = UserA, Title = "Due", DueDate = localDueDate });
            await context.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext(UserA);
        var stored = await verifyContext.TodoTasks.AsNoTracking().SingleAsync();
        stored.DueDate.Should().Be(localDueDate.ToUniversalTime());
        stored.DueDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    private AppDbContext CreateContext(string? userId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new AppDbContext(options, new FakeCurrentUserService(userId));
    }

    private async Task<int> SeedTaskAsync(string userId, string title, int subTaskCount = 0)
    {
        await using var context = CreateContext(userId);
        var task = new TodoTask { UserId = userId, Title = title };
        for (var i = 0; i < subTaskCount; i++)
        {
            task.SubTasks.Add(new SubTask { Title = $"Step {i + 1}", SortOrder = i });
        }

        context.TodoTasks.Add(task);
        await context.SaveChangesAsync();
        return task.Id;
    }
}
