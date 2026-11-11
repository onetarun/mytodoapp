using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using TodoApp.Api.Controllers;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Tasks;
using TodoApp.Api.Services;

namespace TodoApp.Tests.Controllers;

public sealed class TasksControllerTests
{
    private static readonly TaskDetailDto Detail = new(
        5, "Report", Priority.Medium, null, null, null, false, false, 0, 0, null, null, DateTime.UtcNow, DateTime.UtcNow, []);

    private readonly Mock<ITaskService> _service = new();

    [Fact]
    public async Task GetAll_PassesStatus_AndReturns200()
    {
        _service.Setup(s => s.GetAllAsync(TaskStatusFilter.Pending, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await CreateController().GetAllAsync(CancellationToken.None, TaskStatusFilter.Pending);

        result.Should().BeOfType<OkObjectResult>();
        _service.Verify(s => s.GetAllAsync(TaskStatusFilter.Pending, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_UndefinedStatus_Returns400()
    {
        var result = await CreateController().GetAllAsync(CancellationToken.None, (TaskStatusFilter)9);

        var objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        objectResult.Value.Should().BeOfType<ValidationProblemDetails>()
            .Which.Errors.Should().ContainKey(TasksController.StatusField);
        _service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetById_Found_Returns200_AndMissing_Returns404()
    {
        _service.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(Detail);

        (await CreateController().GetByIdAsync(5, CancellationToken.None)).Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(Detail);
        (await CreateController().GetByIdAsync(6, CancellationToken.None)).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_Returns201WithLocation()
    {
        var request = new CreateTaskRequest("Report");
        _service.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(Detail);

        var result = await CreateController().CreateAsync(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().Be("/api/v1/tasks/5");
        created.Value.Should().Be(Detail);
    }

    [Fact]
    public async Task Update_Returns200OrNotFound()
    {
        var request = new UpdateTaskRequest("Report");
        _service.Setup(s => s.UpdateAsync(5, request, It.IsAny<CancellationToken>())).ReturnsAsync(Detail);

        (await CreateController().UpdateAsync(5, request, CancellationToken.None)).Should().BeOfType<OkObjectResult>();
        (await CreateController().UpdateAsync(6, request, CancellationToken.None)).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SetCompletion_Returns200OrNotFound()
    {
        _service.Setup(s => s.SetCompletionAsync(5, true, It.IsAny<CancellationToken>())).ReturnsAsync(Detail);

        (await CreateController().SetCompletionAsync(5, new SetCompletionRequest(true), CancellationToken.None))
            .Should().BeOfType<OkObjectResult>();
        (await CreateController().SetCompletionAsync(6, new SetCompletionRequest(true), CancellationToken.None))
            .Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Returns204OrNotFound()
    {
        _service.Setup(s => s.DeleteAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        (await CreateController().DeleteAsync(5, CancellationToken.None)).Should().BeOfType<NoContentResult>();
        (await CreateController().DeleteAsync(6, CancellationToken.None)).Should().BeOfType<NotFoundResult>();
    }

    private TasksController CreateController()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();

        return new TasksController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() },
            },
        };
    }
}
