using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Payments.Commands.DeclarePayment;

/// <summary>
/// The "I already transferred the money to your bank account" path for a bank
/// with no automatic verifier behind it (see IReceiptVerifier / GET
/// /payments/verifiers for the ones that do).
///
/// This is NOT RecordPaymentCommand and deliberately isn't allowed to become it:
/// it files a PaymentStatus.Pending claim, leaves the invoice balance alone and
/// issues no receipt, so an agent raising one for their own invoice gains
/// nothing until an admin matches it to the bank statement and approves it.
/// That's why this one is open to hotel agents/customers while POST /payments
/// stays SuperAdmin-only.
/// </summary>
public record DeclarePaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod Method,
    string Reference,
    Guid? BankAccountId = null) : IRequest<DeclarePaymentResult>;

public record DeclarePaymentResult(
    Guid PaymentId,
    string PaymentNumber,
    decimal Amount,
    string Reference,
    string Status);
