using MediatR;

namespace FHSMS.Application.Invoices.Commands.GenerateInvoice;

/// <summary>
/// Generates an invoice from a confirmed order. This is the point where the
/// TaxEngine actually runs, line by line, and its results are frozen onto the
/// new Invoice/InvoiceItem rows forever.
/// </summary>
public record GenerateInvoiceCommand(Guid OrderId, decimal Discount = 0) : IRequest<Guid>;
