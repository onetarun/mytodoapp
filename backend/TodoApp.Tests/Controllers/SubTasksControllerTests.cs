using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using Moq;

using TodoApp.Api.Controllers;
using TodoApp.Api.DTOs.SubTasks;
using TodoApp.Api.DTOs.Tasks;
using TodoApp.Api.Services;

namespace TodoApp.Tests.Controllers;

public sealed class SubTasksControllerTests
{
    private static readonly SubTaskDto SubTask = new(9, "Pack", false, 1);

    private readonly Mock<ISubTaskService> _service = new();

    [Fact]
    public async Task Create_Returns201WithLocation_Or404()
    {
        var request = new CreateSubTaskRequest("Pack");
        _service.Setup(s => s.CreateAsync(5, request, It.IsAny<CancellationToken>())).ReturnsAsync(SubTask);
        var controller = new SubTasksController(_service.Object);

        var created = (await controller.CreateAsync(5, request, CancellationToken.None)).Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().Be("/api/v1/tasks/5/subtasks/9");
        created.Value.Should().Be(SubTask);
        (await controller.CreateAsync(6, request, CancellationToken.None)).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_Returns200Or404()
    {
        var request = new UpdateSubTaskRequest("Pack");
        _service.Setup(s => s.UpdateAsync(5, 9, request, It.IsAny<CancellationToken>())).ReturnsAsync(SubTask);
        var controller = new SubTasksController(_service.Object);

        (await controller.UpdateAsync(5, 9, request, CancellationToken.None)).Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SubTask);
        (await controller.UpdateAsync(5, 10, request, CancellationToken.None)).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SetCompletion_Returns200Or404()
    {
        _service.Setup(s => s.SetCompletionAsync(5, 9, true, It.IsAny<CancellationToken>())).ReturnsAsync(SubTask);
        var controller = new SubTasksController(_service.Object);

        (await controller.SetCompletionAsync(5, 9, new SetCompletionRequest(true), CancellationToken.None))
            .Should().BeOfType<OkObjectResult>();
        (await controller.SetCompletionAsync(5, 10, new SetCompletionRequest(true), CancellationToken.None))
            .Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Returns204Or404()
    {
        _service.Setup(s => s.DeleteAsync(5, 9, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new SubTasksController(_service.Object);

        (await controller.DeleteAsync(5, 9, CancellationToken.None)).Should().BeOfType<NoContentResult>();
        (await controller.DeleteAsync(5, 10, CancellationToken.None)).Should().BeOfType<NotFoundResult>();
    }
}
