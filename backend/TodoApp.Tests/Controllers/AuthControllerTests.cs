using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using TodoApp.Api.Controllers;
using TodoApp.Api.DTOs.Auth;
using TodoApp.Api.Services;

namespace TodoApp.Tests.Controllers;

public sealed class AuthControllerTests
{
    private static readonly AuthResponse Response =
        new("token", DateTime.UtcNow.AddMinutes(60), new AuthUserDto("user-1", "Asha", "asha@example.test"));

    private static readonly RegisterRequest RegisterRequest = new("Asha", "asha@example.test", "testpass1");

    private static readonly LoginRequest LoginRequest = new("asha@example.test", "testpass1");

    private readonly Mock<IAuthService> _authService = new();

    [Fact]
    public async Task Register_Success_Returns201WithResponse()
    {
        _authService.Setup(s => s.RegisterAsync(RegisterRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.Success(Response));

        var result = await CreateController().RegisterAsync(RegisterRequest, CancellationToken.None);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        objectResult.Value.Should().Be(Response);
    }

    [Fact]
    public async Task Register_ValidationFailed_Returns400WithFieldErrors()
    {
        _authService.Setup(s => s.RegisterAsync(RegisterRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.ValidationFailed(new Dictionary<string, string[]>
            {
                [AuthService.EmailField] = [AuthService.DuplicateEmailMessage],
            }));

        var result = await CreateController().RegisterAsync(RegisterRequest, CancellationToken.None);

        var objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var problem = objectResult.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problem.Errors[AuthService.EmailField].Should().ContainSingle().Which.Should().Be(AuthService.DuplicateEmailMessage);
    }

    [Fact]
    public async Task Login_Success_Returns200WithResponse()
    {
        _authService.Setup(s => s.LoginAsync(LoginRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.Success(Response));

        var result = await CreateController().LoginAsync(LoginRequest, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(Response);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401WithGenericMessage()
    {
        _authService.Setup(s => s.LoginAsync(LoginRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.InvalidCredentials());

        var result = await CreateController().LoginAsync(LoginRequest, CancellationToken.None);

        var problem = AssertProblem(result, StatusCodes.Status401Unauthorized);
        problem.Title.Should().Be(AuthController.InvalidCredentialsTitle);
    }

    [Fact]
    public async Task Login_LockedOut_Returns423()
    {
        _authService.Setup(s => s.LoginAsync(LoginRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.LockedOut());

        var result = await CreateController().LoginAsync(LoginRequest, CancellationToken.None);

        var problem = AssertProblem(result, StatusCodes.Status423Locked);
        problem.Title.Should().Be(AuthController.LockedOutTitle);
    }

    private static ProblemDetails AssertProblem(IActionResult result, int statusCode)
    {
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(statusCode);
        return objectResult.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
    }

    private AuthController CreateController()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();

        return new AuthController(_authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() },
            },
        };
    }
}
