using MediatR;

namespace FHSMS.Application.Payments.Events;

/// <summary>
/// Published once a *new* payment is successfully recorded against an
/// invoice - manual admin entry, an online Chapa/Telebirr checkout webhook,
/// or a self-service "I already paid" verification all funnel through
/// RecordPaymentCommandHandler, so this fires the same way regardless of
/// which path triggered it. Deliberately NOT published on the idempotent
/// "this reference was already recorded, here's the existing receipt" path -
/// a retried webhook must never re-notify the customer for a payment they
/// already got a receipt for.
/// </summary>
public record PaymentRecordedEvent(Guid PaymentId, Guid InvoiceId, Guid CustomerId, decimal Amount, string PaymentNumber, string ReceiptNumber) : INotification;
