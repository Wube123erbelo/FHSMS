using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Payments.Commands.RecordPayment;

public record RecordPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod Method,
    string? Reference,
    Guid? BankAccountId = null,
    string? ProviderResponse = null) : IRequest<RecordPaymentResult>;

public record RecordPaymentResult(Guid PaymentId, string PaymentNumber, Guid ReceiptId, string ReceiptNumber);
