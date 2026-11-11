using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using TodoApp.Api.Controllers;
using TodoApp.Api.DTOs.Dashboard;
using TodoApp.Api.DTOs.Notifications;
using TodoApp.Api.Services;

namespace TodoApp.Tests.Controllers;

public sealed class DashboardAndNotificationsControllerTests
{
    private static readonly DashboardSummaryDto Summary = new(3, 1, 2, 1, 1, []);

    private readonly Mock<IDashboardService> _dashboard = new();

    [Fact]
    public async Task Summary_WithoutTz_UsesUtc()
    {
        _dashboard.Setup(s => s.GetSummaryAsync(TimeZoneInfo.Utc, It.IsAny<CancellationToken>())).ReturnsAsync(Summary);

        var result = await CreateDashboardController().GetSummaryAsync(null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(Summary);
    }

    [Fact]
    public async Task Summary_WithIanaTz_PassesThatZone()
    {
        _dashboard.Setup(s => s.GetSummaryAsync(It.Is<TimeZoneInfo>(z => z.BaseUtcOffset == TimeSpan.FromMinutes(330)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Summary);

        var result = await CreateDashboardController().GetSummaryAsync("Asia/Kolkata", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Summary_UnknownTz_Returns400()
    {
        var result = await CreateDashboardController().GetSummaryAsync("Mars/Olympus", CancellationToken.None);

        var objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        objectResult.Value.Should().BeOfType<ValidationProblemDetails>()
            .Which.Errors.Should().ContainKey(DashboardController.TimeZoneField);
        _dashboard.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Notifications_Returns200()
    {
        var response = new NotificationsResponse(0, []);
        var service = new Mock<INotificationService>();
        service.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await new NotificationsController(service.Object).GetAsync(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(response);
    }

    private DashboardController CreateDashboardController()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();

        return new DashboardController(_dashboard.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() },
            },
        };
    }
}
