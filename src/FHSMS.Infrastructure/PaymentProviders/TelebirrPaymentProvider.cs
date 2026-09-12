using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FHSMS.Infrastructure.PaymentProviders;

/// <summary>
/// Telebirr's H5 Web Payment integration - built from Telebirr/Ethio Telecom's
/// publicly documented flow shape (third-party pre-order request -> redirect
/// to Telebirr's hosted H5 payment page -> server-to-server notify callback),
/// NOT the generic HMAC-webhook shape PaymentProviderBase assumes (which,
/// like Chapa, this class deliberately does not use).
///
/// *** READ BEFORE GOING LIVE ***
/// Unlike Chapa (which has an open, self-service, publicly documented REST
/// API at developer.chapa.co that this codebase was verified against),
/// Telebirr has no public self-service API docs - integration requires a
/// merchant agreement with Ethio Telecom, who then hand you the exact
/// request/response field names, the signing algorithm's padding scheme,
/// and your environment's real PaymentBaseUrl. This implementation follows
/// the flow shape that is consistently described across multiple
/// third-party integration references (sign every outgoing request with
/// your merchant RSA private key over the sorted "key=value&..." param
/// string, redirect to Telebirr's H5 landing page with appid/sign/the
/// signed payload, verify the notify callback's signature with Telebirr's
/// RSA public key before trusting it) - but those references disagree on
/// some specifics (e.g. RSA-PKCS1 vs RSA-PSS padding, and the exact notify
/// payload's field names). SHA256withRSA/PKCS1 is used below as the most
/// commonly cited default; both the signing padding and the notify field
/// names in TelebirrNotifyPayload below MUST be confirmed against your own
/// merchant packet from Ethio Telecom and tested against their sandbox
/// before a single real transaction goes through this.
/// </summary>
public class TelebirrPaymentProvider : IPaymentProvider, IInitiatablePaymentProvider
{
    private readonly PaymentProviderSettings.TelebirrCredential _credential;
    private readonly PaymentProviderSettings _settings;
    private readonly ILogger<TelebirrPaymentProvider> _logger;

    public TelebirrPaymentProvider(IOptions<PaymentProviderSettings> settings, ILogger<TelebirrPaymentProvider> logger)
    {
        _settings = settings.Value;
        _credential = settings.Value.Telebirr;
        _logger = logger;
    }

    public PaymentMethod Method => PaymentMethod.Telebirr;

    public Task<InitiatePaymentResult> InitiateAsync(Guid invoiceId, decimal amount, string customerEmail, string customerName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_credential.AppId) || string.IsNullOrEmpty(_credential.MerchantPrivateKeyPem) || string.IsNullOrEmpty(_credential.PaymentBaseUrl))
            return Task.FromResult(new InitiatePaymentResult(false, null, null,
                "Telebirr isn't configured (PaymentProviders:Telebirr:AppId/MerchantPrivateKeyPem/PaymentBaseUrl are required)."));

        // The invoice's own ID doubles as outTradeNo - same reasoning as
        // Chapa's tx_ref: it's how the notify callback gets tied back to one
        // of our invoices, since Telebirr has no concept of our InvoiceId.
        var outTradeNo = invoiceId.ToString("N");
        var nonce = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["appId"] = _credential.AppId!,
            ["nonce"] = nonce,
            ["notifyUrl"] = BuildUrl(_settings.ApiPublicBaseUrl, "api/webhooks/payments/telebirr") ?? "",
            ["outTradeNo"] = outTradeNo,
            ["receiveName"] = _credential.ReceiveName ?? "",
            ["returnUrl"] = BuildUrl(_settings.FrontendPublicBaseUrl, $"invoices/{invoiceId}") ?? "",
            ["shortCode"] = _credential.ShortCode ?? "",
            ["subject"] = $"Invoice {outTradeNo}",
            ["timeoutExpress"] = _credential.TimeoutExpressMinutes.ToString(),
            ["timestamp"] = timestamp,
            ["totalAmount"] = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
        };

        string sign;
        try
        {
            sign = SignParameters(parameters, _credential.MerchantPrivateKeyPem!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telebirr: failed to sign pre-order request for invoice {InvoiceId} - check MerchantPrivateKeyPem is a valid PEM key", invoiceId);
            return Task.FromResult(new InitiatePaymentResult(false, null, null, "Could not sign the Telebirr payment request - check the configured private key."));
        }

        // Redirect the payer to Telebirr's hosted H5 landing page with the
        // signed request attached - Telebirr itself renders the payment UI
        // from here, we never see the payer's wallet credentials.
        var query = string.Join("&", parameters.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"))
            + $"&sign={Uri.EscapeDataString(sign)}";
        var checkoutUrl = $"{_credential.PaymentBaseUrl!.TrimEnd('/')}?{query}";

        return Task.FromResult(new InitiatePaymentResult(true, checkoutUrl, outTradeNo, null));
    }

    public Task<PaymentCallbackResult> ParseCallbackAsync(string rawPayload, IDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        TelebirrNotifyPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<TelebirrNotifyPayload>(rawPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Telebirr webhook: malformed JSON payload");
            return Task.FromResult(new PaymentCallbackResult(false, null, 0, null, rawPayload, "Malformed payload."));
        }

        if (payload?.OutTradeNo is not { Length: > 0 } outTradeNo || !Guid.TryParse(outTradeNo, out var invoiceId))
        {
            _logger.LogWarning("Telebirr webhook: outTradeNo missing or not a recognizable invoice ID ({OutTradeNo})", payload?.OutTradeNo);
            return Task.FromResult(new PaymentCallbackResult(false, null, 0, payload?.OutTradeNo, rawPayload, "Unrecognized transaction reference."));
        }

        if (string.IsNullOrEmpty(_credential.TelebirrPublicKeyPem))
        {
            _logger.LogError("Telebirr webhook received but no TelebirrPublicKeyPem is configured - refusing to trust it unverified");
            return Task.FromResult(new PaymentCallbackResult(false, invoiceId, 0, outTradeNo, rawPayload, "Telebirr callback verification isn't configured."));
        }

        // Verify the notify payload's own signature against Telebirr's public
        // key before trusting anything in it - same principle as Chapa's
        // server-to-server verify call, adapted to Telebirr's signed-payload
        // (rather than separate verify-endpoint) notify design.
        var isValid = VerifySignature(payload, _credential.TelebirrPublicKeyPem);
        if (!isValid)
        {
            _logger.LogWarning("Telebirr webhook: signature verification failed for {OutTradeNo}", outTradeNo);
            return Task.FromResult(new PaymentCallbackResult(false, invoiceId, 0, outTradeNo, rawPayload, "Signature validation failed."));
        }

        // TRADE_SUCCESS is the status value consistently described across
        // Telebirr integration references for a completed payment - confirm
        // against your own merchant packet.
        if (!string.Equals(payload.TradeStatus, "TRADE_SUCCESS", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Telebirr webhook: trade status was not success for {OutTradeNo} (status: {Status})", outTradeNo, payload.TradeStatus);
            return Task.FromResult(new PaymentCallbackResult(false, invoiceId, 0, outTradeNo, rawPayload, $"Payment not completed (status: {payload.TradeStatus})."));
        }

        return Task.FromResult(new PaymentCallbackResult(true, invoiceId, payload.TotalAmount, payload.TradeNo ?? outTradeNo, rawPayload, null));
    }

    /// <summary>SHA256withRSA/PKCS1 over "key1=value1&amp;key2=value2..." with keys sorted ordinally ascending - see class remarks on why this specific padding is a best-effort default, not a confirmed spec.</summary>
    private static string SignParameters(SortedDictionary<string, string> parameters, string privateKeyPem)
    {
        var signableString = string.Join("&", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
        var bytes = Encoding.UTF8.GetBytes(signableString);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        var signatureBytes = rsa.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signatureBytes);
    }

    private bool VerifySignature(TelebirrNotifyPayload payload, string publicKeyPem)
    {
        if (string.IsNullOrEmpty(payload.Sign))
            return false;

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["appId"] = payload.AppId ?? "",
            ["outTradeNo"] = payload.OutTradeNo ?? "",
            ["totalAmount"] = payload.TotalAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            ["tradeNo"] = payload.TradeNo ?? "",
            ["tradeStatus"] = payload.TradeStatus ?? ""
        };
        var signableString = string.Join("&", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
        var bytes = Encoding.UTF8.GetBytes(signableString);

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            var signatureBytes = Convert.FromBase64String(payload.Sign);
            return rsa.VerifyData(bytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telebirr: failed to verify notify signature - check TelebirrPublicKeyPem is a valid PEM key");
            return false;
        }
    }

    private static string? BuildUrl(string? baseUrl, string path)
        => string.IsNullOrEmpty(baseUrl) ? null : $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

    /// <summary>
    /// Best-effort shape based on the common third-party-integration
    /// description of Telebirr's notify callback - field names MUST be
    /// confirmed against your actual merchant packet (see class remarks).
    /// </summary>
    private class TelebirrNotifyPayload
    {
        public string? AppId { get; set; }
        public string? OutTradeNo { get; set; }
        public string? TradeNo { get; set; }
        public string? TradeStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Sign { get; set; }
    }
}
