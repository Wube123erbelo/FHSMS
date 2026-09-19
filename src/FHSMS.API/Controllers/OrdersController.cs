using FHSMS.API.Common;
using FHSMS.Application.Common.Models;
using FHSMS.Application.Orders.Commands.CancelOrder;
using FHSMS.Application.Orders.Commands.CreateOrder;
using FHSMS.Application.Orders.Commands.UpdateOrderStatus;
using FHSMS.Application.Orders.Queries.GetOrderById;
using FHSMS.Application.Orders.Queries.GetOrders;
using FHSMS.Application.Orders.Queries.GetOrdersAwaitingDelivery;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// Backs the order state machine exactly as specified:
/// Draft -&gt; Pending -&gt; Confirmed -&gt; Preparing -&gt; Shipped -&gt; Delivered -&gt; Completed,
/// with Reject/Cancel/Return as the exception paths. Each action below maps
/// to exactly one Order domain method, which is the only place a transition
/// can be validated or refused.
/// </summary>
[Authorize]
public class OrdersController : ApiControllerBase
{
    public record ReasonRequest(string? Reason);

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetAll([FromQuery] Guid? customerId, [FromQuery] OrderStatus? status)
        => Ok(await Mediator.Send(new GetOrdersQuery(customerId, status)));

    [HttpGet("export/csv")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportCsv([FromQuery] Guid? customerId, [FromQuery] OrderStatus? status)
    {
        var orders = await Mediator.Send(new GetOrdersQuery(customerId, status));
        var csv = CsvExporter.Write(orders, new (string, Func<OrderDto, object?>)[]
        {
            ("Order number", o => o.OrderNumber),
            ("Customer", o => o.CustomerName),
            ("Status", o => o.Status),
            ("Order date", o => o.OrderDate),
            ("Subtotal", o => o.Subtotal),
            ("Item count", o => o.Items.Count)
        });
        return File(csv, "text/csv", "orders.csv");
    }

    [HttpGet("export/excel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportExcel([FromQuery] Guid? customerId, [FromQuery] OrderStatus? status)
    {
        var orders = await Mediator.Send(new GetOrdersQuery(customerId, status));
        var xls = ExcelExporter.Write("Orders", orders, new (string, Func<OrderDto, object?>)[]
        {
            ("Order number", o => o.OrderNumber),
            ("Customer", o => o.CustomerName),
            ("Status", o => o.Status),
            ("Order date", o => o.OrderDate),
            ("Subtotal", o => o.Subtotal)
        });
        return File(xls, "application/vnd.ms-excel", "orders.xls");
    }

    [HttpGet("export/pdf")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportPdf([FromQuery] Guid? customerId, [FromQuery] OrderStatus? status)
    {
        var orders = await Mediator.Send(new GetOrdersQuery(customerId, status));
        var pdf = PdfTableExporter.Write("Orders", orders, new (string, Func<OrderDto, object?>)[]
        {
            ("Order #", o => o.OrderNumber),
            ("Customer", o => o.CustomerName),
            ("Status", o => o.Status),
            ("Order date", o => o.OrderDate),
            ("Subtotal", o => o.Subtotal)
        });
        return File(pdf, "application/pdf", "orders.pdf");
    }

    /// <summary>Creates an order and immediately submits it (Draft -> Pending) - matches the "select, review, confirm" customer flow in one call.</summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateOrderCommand command)
        => Ok(await Mediator.Send(command));

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid orderId)
        => Ok(await Mediator.Send(new GetOrderByIdQuery(orderId)));

    /// <summary>Paid orders (Confirmed/Preparing/Shipped) with no delivery/trip posted yet - the admin's "Create delivery" order picker. Admin-only, same audience as the delivery-creation screen itself.</summary>
    [HttpGet("awaiting-delivery")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<OrderDto>>> GetAwaitingDelivery()
        => Ok(await Mediator.Send(new GetOrdersAwaitingDeliveryQuery()));

    /// <summary>Pending -> Confirmed. Operations accepts the order. Admin-only: an agent approving their own order would bypass the oversight this step exists for.</summary>
    [HttpPost("{orderId:guid}/confirm")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Confirm(Guid orderId)
    {
        await Mediator.Send(new ConfirmOrderCommand(orderId));
        return NoContent();
    }

    /// <summary>Pending -> Rejected. Operations declines the order. Admin-only, same reasoning as Confirm.</summary>
    [HttpPost("{orderId:guid}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Reject(Guid orderId, [FromBody] ReasonRequest request)
    {
        await Mediator.Send(new RejectOrderCommand(orderId, request.Reason));
        return NoContent();
    }

    /// <summary>Confirmed -> Preparing. Stock is being picked/packed - a warehouse/operations action, not a field-agent one.</summary>
    [HttpPost("{orderId:guid}/prepare")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Prepare(Guid orderId)
    {
        await Mediator.Send(new PrepareOrderCommand(orderId));
        return NoContent();
    }

    /// <summary>Preparing -> Shipped. Also settable indirectly by dispatching the linked Delivery. Admin-only - this is what triggers automatic inventory deduction, so it needs the same control as any stock movement.</summary>
    [HttpPost("{orderId:guid}/ship")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Ship(Guid orderId)
    {
        await Mediator.Send(new ShipOrderCommand(orderId));
        return NoContent();
    }

    /// <summary>Delivered -> Completed. Closes the business transaction - a financial/audit close-out, admin-only.</summary>
    [HttpPost("{orderId:guid}/complete")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Complete(Guid orderId)
    {
        await Mediator.Send(new CompleteOrderCommand(orderId));
        return NoContent();
    }

    /// <summary>Shipped/Delivered -> Returned. Starts the refund/credit workflow - financial impact, admin-only.</summary>
    [HttpPost("{orderId:guid}/return")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Return(Guid orderId, [FromBody] ReasonRequest request)
    {
        await Mediator.Send(new ReturnOrderCommand(orderId, request.Reason));
        return NoContent();
    }

    /// <summary>
    /// Draft/Pending/Confirmed/Preparing -> Cancelled. Not allowed once shipped.
    /// The one order action a field agent keeps: an agent can cancel an
    /// order they themselves placed (e.g. they mis-entered it) before it's
    /// gone anywhere - CancelOrderCommandHandler enforces that ownership
    /// check server-side, since the role attribute alone can't express "only
    /// your own orders".
    /// </summary>
    [HttpPost("{orderId:guid}/cancel")]
    [Authorize(Roles = "SuperAdmin,HotelAgent,FarmerAgent")]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        await Mediator.Send(new CancelOrderCommand(orderId));
        return NoContent();
    }
}
