using MediatR;

namespace FHSMS.Application.Payments.Commands.InitiateInvoicePayment;

/// <summary>
/// Kicks off a hosted-checkout payment (Chapa today; any future provider
/// that implements IInitiatablePaymentProvider automatically works here too,
/// no changes needed) for an invoice's outstanding balance. Returns a
/// checkout URL for the frontend to redirect the customer to - the actual
/// payment gets recorded later when that provider's webhook arrives at
/// PaymentWebhooksController.
/// </summary>
public record InitiateInvoicePaymentCommand(Guid InvoiceId, string ProviderKey) : IRequest<InitiateInvoicePaymentResult>;

public record InitiateInvoicePaymentResult(string CheckoutUrl);
