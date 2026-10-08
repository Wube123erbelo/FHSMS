using MediatR;

namespace FHSMS.Application.Payments.Events;

/// <summary>
/// Raised when someone files a self-service "I already paid" claim. Nothing
/// financial has happened yet - this exists so admins find out there's
/// something waiting for them instead of having to poll the reconciliation
/// screen.
/// </summary>
public record PaymentClaimDeclaredEvent(
    Guid PaymentId,
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Amount,
    string PaymentNumber,
    string Reference,
    Guid? DeclaredByUserId,
    string? DeclaredByEmail) : INotification;

/// <summary>Raised once an admin has approved or rejected a claim, so whoever filed it hears back either way.</summary>
public record PaymentClaimReviewedEvent(
    Guid PaymentId,
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Amount,
    string PaymentNumber,
    bool Approved,
    string? Note,
    string? DeclaredBy) : INotification;
