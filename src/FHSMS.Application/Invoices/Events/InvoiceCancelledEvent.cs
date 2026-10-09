using MediatR;

namespace FHSMS.Application.Invoices.Events;

/// <summary>Published whenever an invoice is cancelled (POST /invoices/{id}/cancel), so the customer can be told rather than discovering it only by noticing their balance changed.</summary>
public record InvoiceCancelledEvent(Guid InvoiceId, Guid CustomerId, string InvoiceNumber) : INotification;
