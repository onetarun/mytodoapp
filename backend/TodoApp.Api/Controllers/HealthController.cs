using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.Controllers;

/// <summary>
/// Temporary smoke-check endpoint (Task 02). Task 04 removes it or keeps it anonymous.
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
