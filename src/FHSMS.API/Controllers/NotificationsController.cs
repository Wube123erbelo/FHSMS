using FHSMS.Application.Common.Models;
using FHSMS.Application.Notifications.Commands.MarkNotificationRead;
using FHSMS.Application.Notifications.Queries.GetMyNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class NotificationsController : ApiControllerBase
{
    /// <summary>The caller's own notifications - no userId parameter, derived from their token. Never exposes another user's notifications.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<List<NotificationDto>>> GetMine()
        => Ok(await Mediator.Send(new GetMyNotificationsQuery()));

    [HttpPost("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid notificationId)
    {
        await Mediator.Send(new MarkNotificationReadCommand(notificationId));
        return NoContent();
    }
}
