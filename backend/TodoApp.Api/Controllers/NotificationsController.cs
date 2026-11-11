using Microsoft.AspNetCore.Mvc;

using TodoApp.Api.DTOs.Notifications;
using TodoApp.Api.Services;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<NotificationsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken) =>
        Ok(await notificationService.GetAsync(cancellationToken));
}
