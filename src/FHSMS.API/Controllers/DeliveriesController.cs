using FHSMS.API.Common;
using FHSMS.Application.Common.Models;
using FHSMS.Application.Deliveries.Commands.ConfirmDeliveryReceipt;
using FHSMS.Application.Deliveries.Commands.CreateDelivery;
using FHSMS.Application.Deliveries.Commands.UpdateDeliveryStatus;
using FHSMS.Application.Deliveries.Queries.GetDeliveries;
using FHSMS.Application.Deliveries.Queries.GetDeliveryByOrder;
using FHSMS.Application.Deliveries.Queries.GetDeliverySignature;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class DeliveriesController : ApiControllerBase
{
    /// <summary>Captured at the moment of marking a delivery Delivered - see MarkDeliveredCommand remarks.</summary>
    public record MarkDeliveredRequest(
        string? Notes, string? ReceivedByName, string? SignatureImageBase64, string? PhotoUrl, double? Latitude, double? Longitude);

    /// <summary>Lists deliveries directly - the admin table. No order ID needed.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<DeliveryDto>>> GetAll([FromQuery] DeliveryStatus? status)
        => Ok(await Mediator.Send(new GetDeliveriesQuery(status)));

    [HttpGet("export/csv")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportCsv([FromQuery] DeliveryStatus? status)
    {
        var deliveries = await Mediator.Send(new GetDeliveriesQuery(status));
        var csv = CsvExporter.Write(deliveries, Columns());
        return File(csv, "text/csv", "deliveries.csv");
    }

    [HttpGet("export/excel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportExcel([FromQuery] DeliveryStatus? status)
    {
        var deliveries = await Mediator.Send(new GetDeliveriesQuery(status));
        var xls = ExcelExporter.Write("Deliveries", deliveries, Columns());
        return File(xls, "application/vnd.ms-excel", "deliveries.xls");
    }

    [HttpGet("export/pdf")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportPdf([FromQuery] DeliveryStatus? status)
    {
        var deliveries = await Mediator.Send(new GetDeliveriesQuery(status));
        var pdf = PdfTableExporter.Write("Deliveries", deliveries, Columns());
        return File(pdf, "application/pdf", "deliveries.pdf");
    }

    private static (string, Func<DeliveryDto, object?>)[] Columns() => new (string, Func<DeliveryDto, object?>)[]
    {
        ("Order #", d => d.OrderNumber),
        ("Destination", d => d.DestinationAddress),
        ("Driver", d => d.DriverName),
        ("Status", d => d.Status),
        ("Trip price", d => d.TripPrice),
        ("Dispatched", d => d.DispatchedAt),
        ("Delivered", d => d.DeliveredAt)
    };

    /// <summary>Posts a delivery/trip for an order - admin-only, since this is what makes a trip appear on the Driver Portal board.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<Guid>> Create(CreateDeliveryCommand command)
        => Ok(await Mediator.Send(command));

    [HttpGet("by-order/{orderId:guid}")]
    public async Task<ActionResult<DeliveryDto?>> GetByOrder(Guid orderId)
        => Ok(await Mediator.Send(new GetDeliveryByOrderQuery(orderId)));

    /// <summary>Preparing -> Shipped side effect. Admin-only - logistics dispatch decision.</summary>
    [HttpPost("{deliveryId:guid}/dispatch")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Dispatch(Guid deliveryId)
    {
        await Mediator.Send(new DispatchDeliveryCommand(deliveryId));
        return NoContent();
    }

    /// <summary>Proof of delivery (recipient name, signature/photo, GPS) is captured here and is never editable afterwards. The assigned driver confirms their own handoff; admin can also do it for support/correction.</summary>
    [HttpPost("{deliveryId:guid}/delivered")]
    [Authorize(Roles = "SuperAdmin,Driver")]
    public async Task<IActionResult> MarkDelivered(Guid deliveryId, [FromBody] MarkDeliveredRequest request)
    {
        await Mediator.Send(new MarkDeliveredCommand(
            deliveryId, request.Notes, request.ReceivedByName, request.SignatureImageBase64,
            request.PhotoUrl, request.Latitude, request.Longitude));
        return NoContent();
    }

    [HttpGet("{deliveryId:guid}/signature")]
    public async Task<ActionResult<string?>> GetSignature(Guid deliveryId)
        => Ok(await Mediator.Send(new GetDeliverySignatureQuery(deliveryId)));

    /// <summary>The hotel side (whoever placed the order, or admin) confirms goods actually arrived - separate from the driver's own delivered claim. Required before the order can be marked Complete.</summary>
    [HttpPost("{deliveryId:guid}/confirm-receipt")]
    [Authorize(Roles = "SuperAdmin,HotelAgent")]
    public async Task<IActionResult> ConfirmReceipt(Guid deliveryId)
    {
        await Mediator.Send(new ConfirmDeliveryReceiptCommand(deliveryId));
        return NoContent();
    }

    public record MarkFailedRequest(string? Notes);

    /// <summary>Admin-only - a failed delivery needs operational follow-up (redispatch, refund, etc.), not something to leave to whoever's on-site.</summary>
    [HttpPost("{deliveryId:guid}/failed")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> MarkFailed(Guid deliveryId, [FromBody] MarkFailedRequest request)
    {
        await Mediator.Send(new MarkDeliveryFailedCommand(deliveryId, request.Notes));
        return NoContent();
    }
}
