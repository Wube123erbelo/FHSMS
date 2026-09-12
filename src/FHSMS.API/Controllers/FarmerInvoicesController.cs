using FHSMS.API.Common;
using FHSMS.Application.Common.Models;
using FHSMS.Application.FarmerInvoices.Queries.GetFarmerInvoices;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// The buying-side counterpart to InvoicesController. Every farmer invoice is
/// auto-generated the moment stock is logged (see
/// RecordInventoryTransactionCommandHandler) - there is no "create" action
/// here, only listing and (via InventoryController's confirm endpoint)
/// approval.
/// </summary>
[Authorize(Roles = "SuperAdmin,FarmerAgent,Driver")]
public class FarmerInvoicesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FarmerInvoiceDto>>> GetAll([FromQuery] Guid? farmerId, [FromQuery] FarmerInvoiceStatus? status)
        => Ok(await Mediator.Send(new GetFarmerInvoicesQuery(farmerId, status)));

    [HttpGet("export/csv")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportCsv([FromQuery] Guid? farmerId, [FromQuery] FarmerInvoiceStatus? status)
    {
        var invoices = await Mediator.Send(new GetFarmerInvoicesQuery(farmerId, status));
        var csv = CsvExporter.Write(invoices, Columns());
        return File(csv, "text/csv", "farmer-invoices.csv");
    }

    [HttpGet("export/excel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportExcel([FromQuery] Guid? farmerId, [FromQuery] FarmerInvoiceStatus? status)
    {
        var invoices = await Mediator.Send(new GetFarmerInvoicesQuery(farmerId, status));
        var xls = ExcelExporter.Write("Farmer Invoices", invoices, Columns());
        return File(xls, "application/vnd.ms-excel", "farmer-invoices.xls");
    }

    [HttpGet("export/pdf")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportPdf([FromQuery] Guid? farmerId, [FromQuery] FarmerInvoiceStatus? status)
    {
        var invoices = await Mediator.Send(new GetFarmerInvoicesQuery(farmerId, status));
        var pdf = PdfTableExporter.Write("Farmer Invoices", invoices, Columns());
        return File(pdf, "application/pdf", "farmer-invoices.pdf");
    }

    private static (string, Func<FarmerInvoiceDto, object?>)[] Columns() => new (string, Func<FarmerInvoiceDto, object?>)[]
    {
        ("Invoice #", i => i.InvoiceNumber),
        ("Product", i => i.ProductName),
        ("Farmer", i => i.FarmerName),
        ("Agent", i => i.AgentName),
        ("Quantity", i => i.Quantity),
        ("Buying price", i => i.BuyingPriceApplied),
        ("Total amount", i => i.TotalAmount),
        ("Status", i => i.Status),
        ("Date", i => i.CreatedAt)
    };
}
