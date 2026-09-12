using FHSMS.Application.Common.Models;
using FHSMS.Application.Inventory.Commands.ConfirmStockReceipt;
using FHSMS.Application.Inventory.Commands.RecordInventoryTransaction;
using FHSMS.Application.Inventory.Queries.GetPendingStockConfirmations;
using FHSMS.Application.Inventory.Queries.GetStockHistory;
using FHSMS.Application.Inventory.Queries.GetStockLevels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class InventoryController : ApiControllerBase
{
    [HttpGet("stock-levels")]
    public async Task<ActionResult<List<StockLevelDto>>> GetStockLevels()
        => Ok(await Mediator.Send(new GetStockLevelsQuery()));

    [HttpGet("products/{productId:guid}/history")]
    public async Task<ActionResult<List<InventoryTransactionDto>>> GetHistory(Guid productId)
        => Ok(await Mediator.Send(new GetStockHistoryQuery(productId)));

    [HttpPost("transactions")]
    [Authorize(Roles = "SuperAdmin,HotelAgent,FarmerAgent")]
    public async Task<ActionResult<Guid>> RecordTransaction(RecordInventoryTransactionCommand command)
        => Ok(await Mediator.Send(command));

    /// <summary>Admin's review queue - farmer-agent stock-ins not yet confirmed as actually arrived.</summary>
    [HttpGet("pending-confirmations")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<InventoryTransactionDto>>> GetPendingConfirmations()
        => Ok(await Mediator.Send(new GetPendingStockConfirmationsQuery()));

    /// <summary>Confirms a farmer agent's stock-in actually arrived (and records whether the farmer was paid) - only then does it count toward stock-on-hand, accrue the agent's commission, or approve the auto-generated farmer invoice.</summary>
    [HttpPost("transactions/{transactionId:guid}/confirm")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ConfirmReceipt(Guid transactionId, [FromBody] ConfirmStockReceiptBody body)
    {
        await Mediator.Send(new ConfirmStockReceiptCommand(transactionId, body.FarmerWasPaid));
        return NoContent();
    }
}

public record ConfirmStockReceiptBody(bool FarmerWasPaid);
