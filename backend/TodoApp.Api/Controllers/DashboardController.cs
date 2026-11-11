using Microsoft.AspNetCore.Mvc;

using TodoApp.Api.DTOs.Dashboard;
using TodoApp.Api.Services;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    public const string TimeZoneField = "tz";

    public const string UnknownTimeZoneMessage = "Unknown time zone. Use an IANA id such as Asia/Kolkata.";

    /// <param name="tz">The user's time zone (IANA id, e.g. <c>Asia/Kolkata</c>). UTC when missing.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    [HttpGet("summary")]
    [ProducesResponseType<DashboardSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSummaryAsync(string? tz, CancellationToken cancellationToken)
    {
        var timeZone = TimeZoneInfo.Utc;
        if (!string.IsNullOrWhiteSpace(tz) && !TimeZoneInfo.TryFindSystemTimeZoneById(tz, out timeZone))
        {
            ModelState.AddModelError(TimeZoneField, UnknownTimeZoneMessage);
            return ValidationProblem(ModelState);
        }

        return Ok(await dashboardService.GetSummaryAsync(timeZone, cancellationToken));
    }
}
