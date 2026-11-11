using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.DTOs.SubTasks;
using TodoApp.Api.DTOs.Tasks;
using TodoApp.Api.Services;
using TodoApp.Tests.Data;

namespace TodoApp.Tests.Services;

public sealed class SubTaskServiceTests
{
    private const string UserA = "user-a";
    private const string UserB = "user-b";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task AddingThreeSubTasks_GivesSortOrders1To3_AndDetailListsThemInOrder()
    {
        var taskId = await CreateTaskAsync(UserA, "Trip");

        var first = await AddAsync(UserA, taskId, "  Book flight  ");
        var second = await AddAsync(UserA, taskId, "Book hotel");
        var third = await AddAsync(UserA, taskId, "Pack");

        new[] { first.SortOrder, second.SortOrder, third.SortOrder }.Should().Equal(1, 2, 3);
        first.Title.Should().Be("Book flight");
        first.IsCompleted.Should().BeFalse();

        var detail = await WithTaskServiceAsync(UserA, s => s.GetByIdAsync(taskId));
        detail!.SubTasks.Select(s => s.Title).Should().Equal("Book flight", "Book hotel", "Pack");
    }

    [Fact]
    public async Task TickingTwoOfThree_GivesProgress2Of3_InListAndDetail()
    {
        var taskId = await CreateTaskAsync(UserA, "Trip");
        var a = await AddAsync(UserA, taskId, "A");
        var b = await AddAsync(UserA, taskId, "B");
        await AddAsync(UserA, taskId, "C");

        (await WithServiceAsync(UserA, s => s.SetCompletionAsync(taskId, a.Id, true)))!.IsCompleted.Should().BeTrue();
        await WithServiceAsync(UserA, s => s.SetCompletionAsync(taskId, b.Id, true));

        var item = (await WithTaskServiceAsync(UserA, s => s.GetAllAsync(TaskStatusFilter.All))).Single();
        item.SubTaskCompleted.Should().Be(2);
        item.SubTaskTotal.Should().Be(3);
        var detail = await WithTaskServiceAsync(UserA, s => s.GetByIdAsync(taskId));
        detail!.SubTaskCompleted.Should().Be(2);
        detail.SubTaskTotal.Should().Be(3);
    }

    [Fact]
    public async Task Untick_SetsSubTaskBackToNotCompleted()
    {
        var taskId = await CreateTaskAsync(UserA, "Trip");
        var a = await AddAsync(UserA, taskId, "A");
        await WithServiceAsync(UserA, s => s.SetCompletionAsync(taskId, a.Id, true));

        var result = await WithServiceAsync(UserA, s => s.SetCompletionAsync(taskId, a.Id, false));

        result!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ChangesTitle()
    {
        var taskId = await CreateTaskAsync(UserA, "Trip");
        var a = await AddAsync(UserA, taskId, "Old");

        var result = await WithServiceAsync(UserA, s => s.UpdateAsync(taskId, a.Id, new UpdateSubTaskRequest(" New ")));

        result.Should().Be(new SubTaskDto(a.Id, "New", false, 1));
    }

    [Fact]
    public async Task DeletedSubTask_IsSoftDeleted_AndExcludedFromDetailAndProgress()
    {
        var taskId = await CreateTaskAsync(UserA, "Trip");
        var a = await AddAsync(UserA, taskId, "A");
        await AddAsync(UserA, taskId, "B");
        await WithServiceAsync(UserA, s => s.SetCompletionAsync(taskId, a.Id, true));

        (await WithServiceAsync(UserA, s => s.DeleteAsync(taskId, a.Id))).Should().BeTrue();

        var detail = await WithTaskServiceAsync(UserA, s => s.GetByIdAsync(taskId));
        detail!.SubTasks.Select(s => s.Title).Should().Equal("B");
        detail.SubTaskTotal.Should().Be(1);
        detail.SubTaskCompleted.Should().Be(0);
        (await WithServiceAsync(UserA, s => s.DeleteAsync(taskId, a.Id))).Should().BeFalse();
        (await WithServiceAsync(UserA, s => s.UpdateAsync(taskId, a.Id, new UpdateSubTaskRequest("X")))).Should().BeNull();

        await using var context = CreateContext(UserA);
        var stored = await context.SubTasks.IgnoreQueryFilters().SingleAsync(s => s.Id == a.Id);
        stored.IsDeleted.Should().BeTrue();
        stored.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UserB_GetsNotFound_OnUserATask_ForAddEditCompleteAndDelete()
    {
        var taskId = await CreateTaskAsync(UserA, "Private");
        var a = await AddAsync(UserA, taskId, "A");

        (await WithServiceAsync(UserB, s => s.CreateAsync(taskId, new CreateSubTaskRequest("Hack")))).Should().BeNull();
        (await WithServiceAsync(UserB, s => s.UpdateAsync(taskId, a.Id, new UpdateSubTaskRequest("Hack")))).Should().BeNull();
        (await WithServiceAsync(UserB, s => s.SetCompletionAsync(taskId, a.Id, true))).Should().BeNull();
        (await WithServiceAsync(UserB, s => s.DeleteAsync(taskId, a.Id))).Should().BeFalse();

        var detail = await WithTaskServiceAsync(UserA, s => s.GetByIdAsync(taskId));
        detail!.SubTasks.Should().ContainSingle().Which.Should().Be(new SubTaskDto(a.Id, "A", false, 1));
    }

    [Fact]
    public async Task SubTaskOfAnotherTask_ReturnsNotFound()
    {
        var taskOne = await CreateTaskAsync(UserA, "One");
        var taskTwo = await CreateTaskAsync(UserA, "Two");
        var onTaskTwo = await AddAsync(UserA, taskTwo, "Belongs to two");

        (await WithServiceAsync(UserA, s => s.UpdateAsync(taskOne, onTaskTwo.Id, new UpdateSubTaskRequest("X")))).Should().BeNull();
        (await WithServiceAsync(UserA, s => s.SetCompletionAsync(taskOne, onTaskTwo.Id, true))).Should().BeNull();
        (await WithServiceAsync(UserA, s => s.DeleteAsync(taskOne, onTaskTwo.Id))).Should().BeFalse();
    }

    [Fact]
    public async Task UnknownOrDeletedParentTask_ReturnsNotFound()
    {
        var taskId = await CreateTaskAsync(UserA, "Soon deleted");
        var a = await AddAsync(UserA, taskId, "A");
        await WithTaskServiceAsync(UserA, s => s.DeleteAsync(taskId));

        (await WithServiceAsync(UserA, s => s.CreateAsync(999, new CreateSubTaskRequest("X")))).Should().BeNull();
        (await WithServiceAsync(UserA, s => s.CreateAsync(taskId, new CreateSubTaskRequest("X")))).Should().BeNull();
        (await WithServiceAsync(UserA, s => s.SetCompletionAsync(taskId, a.Id, true))).Should().BeNull();
    }

    private async Task<int> CreateTaskAsync(string userId, string title) =>
        await WithTaskServiceAsync(userId, async s => (await s.CreateAsync(new CreateTaskRequest(title))).Id);

    private async Task<SubTaskDto> AddAsync(string userId, int taskId, string title)
    {
        var result = await WithServiceAsync(userId, s => s.CreateAsync(taskId, new CreateSubTaskRequest(title)));
        result.Should().NotBeNull();
        return result!;
    }

    private async Task<T> WithServiceAsync<T>(string userId, Func<SubTaskService, Task<T>> action)
    {
        await using var context = CreateContext(userId);
        return await action(new SubTaskService(context));
    }

    private async Task<T> WithTaskServiceAsync<T>(string userId, Func<TaskService, Task<T>> action)
    {
        var currentUser = new FakeCurrentUserService(userId);
        await using var context = new AppDbContext(Options(), currentUser);
        return await action(new TaskService(context, currentUser));
    }

    private AppDbContext CreateContext(string userId) => new(Options(), new FakeCurrentUserService(userId));

    private DbContextOptions<AppDbContext> Options() =>
        new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options;
}
