using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using TodoApp.Api.DTOs.Auth;
using TodoApp.Api.Extensions;
using TodoApp.Api.Services;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    public const string InvalidCredentialsTitle = "Invalid email or password.";

    public const string LockedOutTitle = "Account locked.";

    public const string LockedOutDetail = "Too many failed login attempts. Please try again later.";

    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        if (result.Status == AuthResultStatus.Success)
        {
            return StatusCode(StatusCodes.Status201Created, result.Response);
        }

        foreach (var (field, messages) in result.Errors)
        {
            foreach (var message in messages)
            {
                ModelState.AddModelError(field, message);
            }
        }

        return ValidationProblem(ModelState);
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Response),
            AuthResultStatus.LockedOut => Problem(
                statusCode: StatusCodes.Status423Locked,
                title: LockedOutTitle,
                detail: LockedOutDetail),
            _ => Problem(statusCode: StatusCodes.Status401Unauthorized, title: InvalidCredentialsTitle),
        };
    }
}
