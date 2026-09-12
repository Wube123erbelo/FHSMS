namespace FHSMS.Infrastructure.PaymentProviders;

/// <summary>
/// One entry per configured provider: the shared secret used to validate its
/// webhook signature, and which header carries that signature. Real
/// deployments set these via user-secrets/environment variables, never
/// committed to appsettings.json.
/// </summary>
public class PaymentProviderSettings
{
    public const string SectionName = "PaymentProviders";

    /// <summary>This API's own public base URL (e.g. https://fhsms-api.onrender.com), no trailing slash needed - used to build the callback_url passed to providers that need to know where to send their webhook (Chapa's /transaction/initialize call, for example).</summary>
    public string? ApiPublicBaseUrl { get; set; }
    /// <summary>The frontend's public base URL - used to build the return_url a customer is redirected to once they've finished paying.</summary>
    public string? FrontendPublicBaseUrl { get; set; }

    public ProviderCredential BankTransfer { get; set; } = new();
    public TelebirrCredential Telebirr { get; set; } = new();
    public ProviderCredential Chapa { get; set; } = new();
    public ProviderCredential CbeBirr { get; set; } = new();

    public class ProviderCredential
    {
        public string? Secret { get; set; }
        public string SignatureHeader { get; set; } = "X-Signature";
    }

    /// <summary>
    /// Telebirr's H5 Web Payment flow is RSA-signed, not the simple shared-secret
    /// HMAC scheme every other provider here uses, so it needs its own shape.
    /// All of these come from Ethio Telecom/Telebirr's merchant onboarding packet -
    /// there is no public self-service signup like Chapa's.
    /// </summary>
    public class TelebirrCredential
    {
        /// <summary>Uniquely identifies this merchant to Telebirr - issued at onboarding.</summary>
        public string? AppId { get; set; }
        /// <summary>Telebirr's merchant short code for this business.</summary>
        public string? ShortCode { get; set; }
        /// <summary>Display name shown to the payer as the receiving party.</summary>
        public string? ReceiveName { get; set; }
        /// <summary>PEM-encoded RSA private key issued to this merchant - signs every outgoing pre-order request.</summary>
        public string? MerchantPrivateKeyPem { get; set; }
        /// <summary>PEM-encoded RSA public key Telebirr provides - verifies the signature on incoming notify callbacks so a forged webhook can't credit an invoice.</summary>
        public string? TelebirrPublicKeyPem { get; set; }
        /// <summary>The H5 web payment landing page base URL for your environment (sandbox vs production) - provided at onboarding, NOT the same across merchants.</summary>
        public string? PaymentBaseUrl { get; set; }
        public int TimeoutExpressMinutes { get; set; } = 30;
    }
}
