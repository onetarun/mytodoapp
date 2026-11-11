using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using TodoApp.Api.Controllers;

namespace TodoApp.Tests.Controllers;

public sealed class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsOk()
    {
        var controller = new HealthController();

        var result = controller.Get();

        result.Should().BeOfType<OkResult>();
    }
}
