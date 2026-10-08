using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Payments.Commands.VerifyAndRecordPayment;

/// <summary>
/// Verifies a payment reference the customer already has (see IReceiptVerifier)
/// and, only if that verification succeeds, records it via the exact same
/// RecordPaymentCommand path a staff member's manual entry or a provider
/// webhook would use - so "how the payment got confirmed" never forks the
/// business rule of "what a payment does to an invoice".
/// </summary>
public record VerifyAndRecordPaymentCommand(
    Guid InvoiceId,
    string ProviderKey,
    string Reference,
    string? SecondaryIdentifier,
    Guid? BankAccountId = null) : IRequest<VerifyAndRecordPaymentResult>;

public record VerifyAndRecordPaymentResult(Guid PaymentId, string PaymentNumber, Guid ReceiptId, string ReceiptNumber);
