using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Interfaces;

/// <summary>
/// Ethiopia's major payment rails (CBE, CBE Birr, and most bank wallets)
/// don't offer small merchants a hosted-checkout redirect the way Chapa
/// does, or a webhook the way a formal Telebirr merchant agreement does.
/// What they DO offer is a public "share receipt" page a customer can point
/// you to after they've already paid you directly - a transaction
/// reference (plus sometimes a phone number or account suffix) that can be
/// looked up and confirmed. This is the "I already transferred the money -
/// here's my reference" flow: the customer pays through their own banking
/// app straight to our account/short code, then submits the reference here
/// instead of us redirecting them anywhere.
///
/// This is a DIFFERENT capability from IInitiatablePaymentProvider - a
/// provider can implement either, both, or neither. CbeBirrPaymentProvider
/// implements this one; ChapaPaymentProvider implements the other; Telebirr
/// implements both (its RSA/H5 hosted flow AND, as a fallback for merchants
/// without that agreement set up yet, could add this too).
/// </summary>
public interface IReceiptVerifier
{
    /// <summary>URL-friendly key used to resolve this verifier - "cbe", "cbebirr", etc.</summary>
    string ProviderKey { get; }

    /// <summary>
    /// Fetches the provider's own public receipt/verification page for this
    /// reference and confirms it represents a real, completed payment of at
    /// least expectedAmount. Never throws for "not found" or "amount
    /// mismatch" - those are legitimate outcomes, not exceptions. Does
    /// throw (let the caller catch) only for genuinely unexpected failures
    /// like the endpoint being unreachable, since Ethiopian banks'
    /// verification endpoints are sometimes only reachable from Ethiopian
    /// IP addresses - that failure needs to be told apart from "the
    /// transaction doesn't exist" so the UI can suggest falling back to
    /// manual recording instead of implying the payment itself is invalid.
    /// </summary>
    Task<PaymentCallbackResult> VerifyAsync(string reference, string? secondaryIdentifier, decimal expectedAmount, CancellationToken cancellationToken);
}

/// <summary>Resolves an IReceiptVerifier by its provider key, same pattern as IPaymentProviderRegistry.</summary>
public interface IReceiptVerifierRegistry
{
    IReceiptVerifier? Resolve(string providerKey);
    IReadOnlyCollection<string> AvailableProviderKeys { get; }
}
