using FHSMS.API.Common;
using FHSMS.Application.Common.Models;
using FHSMS.Application.DriverPayments.Commands.ApproveDriverPayment;
using FHSMS.Application.DriverPayments.Queries.GetDriverPayments;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// The driver-side counterpart to FarmerInvoicesController. Every driver
/// payment is auto-generated the moment a delivery is marked Delivered (see
/// MarkDeliveredCommandHandler) - there is no "create" action here, only
/// listing and approval.
/// </summary>
[Authorize(Roles = "SuperAdmin,Driver")]
public class DriverPaymentsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DriverPaymentDto>>> GetAll([FromQuery] DriverPaymentStatus? status)
        => Ok(await Mediator.Send(new GetDriverPaymentsQuery(status)));

    [HttpGet("export/csv")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportCsv([FromQuery] DriverPaymentStatus? status)
    {
        var payments = await Mediator.Send(new GetDriverPaymentsQuery(status));
        var csv = CsvExporter.Write(payments, Columns());
        return File(csv, "text/csv", "driver-payments.csv");
    }

    [HttpGet("export/excel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportExcel([FromQuery] DriverPaymentStatus? status)
    {
        var payments = await Mediator.Send(new GetDriverPaymentsQuery(status));
        var xls = ExcelExporter.Write("Driver Payments", payments, Columns());
        return File(xls, "application/vnd.ms-excel", "driver-payments.xls");
    }

    [HttpGet("export/pdf")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportPdf([FromQuery] DriverPaymentStatus? status)
    {
        var payments = await Mediator.Send(new GetDriverPaymentsQuery(status));
        var pdf = PdfTableExporter.Write("Driver Payments", payments, Columns());
        return File(pdf, "application/pdf", "driver-payments.pdf");
    }

    private static (string, Func<DriverPaymentDto, object?>)[] Columns() => new (string, Func<DriverPaymentDto, object?>)[]
    {
        ("Driver", p => p.DriverName),
        ("Destination", p => p.DestinationAddress),
        ("Amount", p => p.Amount),
        ("Status", p => p.Status),
        ("Driver paid", p => p.DriverWasPaid),
        ("Date", p => p.CreatedAt)
    };

    [HttpPost("{driverPaymentId:guid}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Approve(Guid driverPaymentId, ApproveDriverPaymentBody body)
    {
        await Mediator.Send(new ApproveDriverPaymentCommand(driverPaymentId, body.DriverWasPaid));
        return NoContent();
    }
}

public record ApproveDriverPaymentBody(bool DriverWasPaid);
