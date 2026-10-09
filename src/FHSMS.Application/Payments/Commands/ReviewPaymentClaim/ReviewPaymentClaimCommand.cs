using MediatR;

namespace FHSMS.Application.Payments.Commands.ReviewPaymentClaim;

/// <summary>
/// An admin's verdict on a self-declared payment claim, after checking it
/// against the bank statement. Approving routes through RecordPaymentCommand so
/// the "what a payment does to an invoice" rule stays in exactly one place;
/// rejecting just marks the claim failed and leaves the balance where it was.
/// </summary>
public record ReviewPaymentClaimCommand(
    Guid PaymentId,
    bool Approve,
    decimal? ConfirmedAmount = null,
    string? Note = null) : IRequest<ReviewPaymentClaimResult>;

public record ReviewPaymentClaimResult(
    Guid PaymentId,
    string PaymentNumber,
    string Status,
    Guid? ReceiptId,
    string? ReceiptNumber);
