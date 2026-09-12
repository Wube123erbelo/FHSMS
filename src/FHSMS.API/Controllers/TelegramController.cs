using FHSMS.Application.Orders.Commands.CreateOrder;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// Webhook target for a Telegram bot (e.g. via Telegram.Bot library, not wired up
/// here). The point of this stub is structural: whatever the bot parses out of a
/// chat message must be translated into the exact same CreateOrderCommand that
/// the web PWA and hotel agents use - there is no separate "Telegram order" rule
/// set. Wire your bot's update handling into this action.
/// </summary>
[AllowAnonymous]
[Route("api/telegram")]
public class TelegramController : ApiControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] object update)
    {
        // TODO: parse the Telegram Update payload (see Telegram.Bot docs), resolve
        // the sending chat to a Customer record, then build and send a
        // CreateOrderCommand exactly like OrdersController.Create does, e.g.:
        //
        // var command = new CreateOrderCommand(customerId, OrderSourceType.Telegram, null, items);
        // await Mediator.Send(command);

        return Ok();
    }
}
