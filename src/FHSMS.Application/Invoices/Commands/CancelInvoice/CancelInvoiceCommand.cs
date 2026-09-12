using MediatR;

namespace FHSMS.Application.Invoices.Commands.CancelInvoice;

/// <summary>
/// Cancels an invoice (e.g. issued in error). Like orders, this is a state
/// transition, never a delete - the invoice and its tax snapshot remain for
/// audit purposes, just marked Cancelled instead of removed.
/// </summary>
public record CancelInvoiceCommand(Guid InvoiceId) : IRequest;
