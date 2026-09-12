using FHSMS.API.Common;
using FHSMS.Application.Common.Models;
using FHSMS.Application.Invoices.Commands.CancelInvoice;
using FHSMS.Application.Invoices.Commands.GenerateInvoice;
using FHSMS.Application.Invoices.Queries.GetInvoiceById;
using FHSMS.Application.Invoices.Queries.GetInvoices;
using FHSMS.Application.Payments.Commands.InitiateInvoicePayment;
using FHSMS.Application.Payments.Commands.VerifyAndRecordPayment;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class InvoicesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<InvoiceDto>>> GetAll([FromQuery] Guid? customerId, [FromQuery] InvoiceStatus? status)
        => Ok(await Mediator.Send(new GetInvoicesQuery(customerId, status)));

    [HttpGet("export/excel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportExcel([FromQuery] Guid? customerId, [FromQuery] InvoiceStatus? status)
    {
        var invoices = await Mediator.Send(new GetInvoicesQuery(customerId, status));
        var xls = ExcelExporter.Write("Invoices", invoices, new (string, Func<InvoiceDto, object?>)[]
        {
            ("Invoice number", i => i.InvoiceNumber),
            ("Date", i => i.InvoiceDate),
            ("Status", i => i.Status),
            ("Subtotal", i => i.Subtotal),
            ("Tax", i => i.TaxAmount),
            ("Platform commission", i => i.PlatformCommissionAmount),
            ("Hotel agent bonus", i => i.HotelAgentBonusAmount),
            ("Grand total", i => i.GrandTotal),
            ("Paid", i => i.AmountPaid),
            ("Balance due", i => i.BalanceDue)
        });
        return File(xls, "application/vnd.ms-excel", "invoices.xls");
    }

    [HttpGet("export/csv")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportCsv([FromQuery] Guid? customerId, [FromQuery] InvoiceStatus? status)
    {
        var invoices = await Mediator.Send(new GetInvoicesQuery(customerId, status));
        var csv = CsvExporter.Write(invoices, new (string, Func<InvoiceDto, object?>)[]
        {
            ("Invoice number", i => i.InvoiceNumber),
            ("Date", i => i.InvoiceDate),
            ("Status", i => i.Status),
            ("Subtotal", i => i.Subtotal),
            ("Tax", i => i.TaxAmount),
            ("Platform commission", i => i.PlatformCommissionAmount),
            ("Hotel agent bonus", i => i.HotelAgentBonusAmount),
            ("Grand total", i => i.GrandTotal),
            ("Paid", i => i.AmountPaid),
            ("Balance due", i => i.BalanceDue)
        });
        return File(csv, "text/csv", "invoices.csv");
    }

    /// <summary>The invoice LIST as a table - one row per invoice. For a single invoice's full printable layout, see GET /{invoiceId}/pdf below.</summary>
    [HttpGet("export/pdf")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportPdf([FromQuery] Guid? customerId, [FromQuery] InvoiceStatus? status)
    {
        var invoices = await Mediator.Send(new GetInvoicesQuery(customerId, status));
        var pdf = PdfTableExporter.Write("Invoices", invoices, new (string, Func<InvoiceDto, object?>)[]
        {
            ("Invoice #", i => i.InvoiceNumber),
            ("Date", i => i.InvoiceDate),
            ("Status", i => i.Status),
            ("Grand total", i => i.GrandTotal),
            ("Balance due", i => i.BalanceDue)
        });
        return File(pdf, "application/pdf", "invoices.pdf");
    }

    /// <summary>
    /// Generates an invoice from a confirmed order. This is where the configurable
    /// Tax/VAT engine runs - the response reflects whatever Settings -> Tax & VAT
    /// currently has configured, with no hard-coded rate anywhere in the call chain.
    /// Admin-only: billing is a financial function, kept separate from whichever
    /// agent placed the order that's being invoiced.
    /// </summary>
    [HttpPost("generate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<Guid>> Generate(GenerateInvoiceCommand command)
        => Ok(await Mediator.Send(command));

    [HttpGet("{invoiceId:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid invoiceId)
        => Ok(await Mediator.Send(new GetInvoiceByIdQuery(invoiceId)));

    [HttpGet("{invoiceId:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid invoiceId)
    {
        var invoice = await Mediator.Send(new GetInvoiceByIdQuery(invoiceId));
        var pdfBytes = InvoicePdfBuilder.Build(invoice);
        return File(pdfBytes, "application/pdf", $"{invoice.InvoiceNumber}.pdf");
    }

    [HttpPost("{invoiceId:guid}/cancel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Cancel(Guid invoiceId)
    {
        await Mediator.Send(new CancelInvoiceCommand(invoiceId));
        return NoContent();
    }

    /// <summary>
    /// Starts a hosted-checkout payment (Chapa today) for this invoice's
    /// outstanding balance and returns a URL to redirect to. The payment
    /// itself gets recorded later when the provider's webhook lands at
    /// PaymentWebhooksController - this call only kicks the flow off.
    /// </summary>
    [HttpPost("{invoiceId:guid}/pay/{providerKey}")]
    public async Task<ActionResult<InitiateInvoicePaymentResult>> Pay(Guid invoiceId, string providerKey)
        => Ok(await Mediator.Send(new InitiateInvoicePaymentCommand(invoiceId, providerKey)));

    public record VerifyPaymentRequest(string ProviderKey, string Reference, string? SecondaryIdentifier, Guid? BankAccountId);

    /// <summary>
    /// The "I already transferred the money - here's my reference" self-service
    /// path: verifies the reference against the provider's own public receipt
    /// lookup (see IReceiptVerifier) and, only if that succeeds, records the
    /// payment immediately - no waiting on a webhook, no admin step. Use
    /// GET /payments/verifiers to see which provider keys are available;
    /// anything not listed there (most individual banks) still needs to go
    /// through the normal "record payment" + reconciliation path.
    /// </summary>
    [HttpPost("{invoiceId:guid}/verify-payment")]
    public async Task<ActionResult<VerifyAndRecordPaymentResult>> VerifyPayment(Guid invoiceId, VerifyPaymentRequest request)
        => Ok(await Mediator.Send(new VerifyAndRecordPaymentCommand(
            invoiceId, request.ProviderKey, request.Reference, request.SecondaryIdentifier, request.BankAccountId)));
}
