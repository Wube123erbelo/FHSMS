using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Interfaces;

/// <summary>
/// Result of parsing/validating one payment provider's callback payload,
/// normalized to the shape RecordPaymentCommand needs regardless of which
/// provider sent it.
/// </summary>
public record PaymentCallbackResult(
    bool IsValid,
    Guid? InvoiceId,
    decimal Amount,
    string? ProviderReference,
    string RawPayload,
    string? FailureReason);

/// <summary>
/// One implementation per payment provider (Bank, Telebirr, Chapa, CBE Birr,
/// future ones). This is the abstraction the roadmap calls for in
/// "Payment and banking architecture": adding a new provider means adding a
/// new class that implements this interface and registering it in DI -
/// PaymentWebhooksController and RecordPaymentCommandHandler never change.
/// </summary>
public interface IPaymentProvider
{
    PaymentMethod Method { get; }

    /// <summary>
    /// Validates the incoming webhook's signature/secret and extracts a
    /// normalized result. Implementations must NOT trust the payload's
    /// invoice/amount fields until the signature check passes.
    /// </summary>
    Task<PaymentCallbackResult> ParseCallbackAsync(string rawPayload, IDictionary<string, string> headers, CancellationToken cancellationToken);
}

/// <summary>Resolves the right IPaymentProvider by method/key without callers needing a switch statement.</summary>
public interface IPaymentProviderRegistry
{
    IPaymentProvider Resolve(string providerKey);
    IReadOnlyCollection<string> AvailableProviderKeys { get; }
}

public record InitiatePaymentResult(bool Success, string? CheckoutUrl, string? ProviderReference, string? FailureReason);

/// <summary>
/// Optional capability for providers that support a hosted-checkout redirect
/// flow (Chapa and Telebirr do; BankTransfer is manual by nature and CBE
/// Birr isn't wired up to this yet - see CbeBirrPaymentProvider's remarks).
/// A provider implements this in addition to IPaymentProvider; callers
/// resolve it normally via IPaymentProviderRegistry and pattern-match with
/// `as IInitiatablePaymentProvider` to see if this particular one supports it.
/// </summary>
public interface IInitiatablePaymentProvider
{
    Task<InitiatePaymentResult> InitiateAsync(Guid invoiceId, decimal amount, string customerEmail, string customerName, CancellationToken cancellationToken);
}
