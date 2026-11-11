using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.Controllers;

/// <summary>
/// Anonymous smoke-check endpoint (Task 02). Kept anonymous in Task 04.
/// </summary>
[ApiController]
[Route("api/v1/health")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() => Ok();
}
