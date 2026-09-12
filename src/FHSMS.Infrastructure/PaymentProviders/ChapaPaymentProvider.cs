using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FHSMS.Infrastructure.PaymentProviders;

/// <summary>
/// Real Chapa integration - initiate + verify + webhook, matching Chapa's
/// actual public API (developer.chapa.co) rather than the generic shape
/// PaymentProviderBase assumes (which none of the four providers' real APIs
/// share, and which this class deliberately does NOT use). Verified against
/// Chapa's docs at the time this was written; payment provider APIs do
/// change, so it's worth a quick recheck against developer.chapa.co before
/// a first real transaction.
///
/// Key facts this implementation is built on:
///  - Initialize: POST https://api.chapa.co/v1/transaction/initialize,
///    Bearer <secret key>, body includes a caller-chosen unique tx_ref -
///    that's how a webhook/verify response gets tied back to one of our
///    invoices later, since Chapa has no concept of our own InvoiceId. This
///    class uses the invoice's own Guid as tx_ref for exactly that reason.
///  - Verify: GET https://api.chapa.co/v1/transaction/verify/{tx_ref},
///    same Bearer auth. Chapa's own docs recommend always re-verifying here
///    rather than trusting a webhook payload's amount/status directly -
///    that's what ParseCallbackAsync does below before ever returning
///    IsValid=true.
///  - Webhook signature: header x-chapa-signature is HMAC-SHA256 of the raw
///    payload using the secret key; header chapa-signature is HMAC-SHA256
///    of the secret key itself (constant across every call, so on its own
///    it only proves the caller knows the secret - not that this specific
///    payload is untampered). Chapa's docs accept either header matching.
/// </summary>
public class ChapaPaymentProvider : IPaymentProvider, IInitiatablePaymentProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;
    private readonly PaymentProviderSettings.ProviderCredential _credential;
    private readonly PaymentProviderSettings _settings;
    private readonly ILogger<ChapaPaymentProvider> _logger;

    public ChapaPaymentProvider(HttpClient http, IOptions<PaymentProviderSettings> settings, ILogger<ChapaPaymentProvider> logger)
    {
        _http = http;
        _settings = settings.Value;
        _credential = settings.Value.Chapa;
        _logger = logger;

        if (!string.IsNullOrEmpty(_credential.Secret))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credential.Secret);
    }

    public PaymentMethod Method => PaymentMethod.Chapa;

    public async Task<InitiatePaymentResult> InitiateAsync(Guid invoiceId, decimal amount, string customerEmail, string customerName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_credential.Secret))
            return new InitiatePaymentResult(false, null, null, "Chapa isn't configured (PaymentProviders:Chapa:Secret is empty).");

        // The invoice's own ID doubles as tx_ref - see class remarks.
        var txRef = invoiceId.ToString();
        var nameParts = customerName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var body = new
        {
            amount = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            currency = "ETB",
            email = customerEmail,
            first_name = nameParts.ElementAtOrDefault(0) ?? customerName,
            last_name = nameParts.ElementAtOrDefault(1) ?? customerName,
            tx_ref = txRef,
            callback_url = BuildUrl(_settings.ApiPublicBaseUrl, "api/webhooks/payments/chapa"),
            return_url = BuildUrl(_settings.FrontendPublicBaseUrl, $"invoices/{invoiceId}")
        };

        HttpResponseMessage response;
        string raw;
        try
        {
            response = await _http.PostAsJsonAsync("transaction/initialize", body, cancellationToken);
            raw = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chapa initialize call failed for invoice {InvoiceId}", invoiceId);
            return new InitiatePaymentResult(false, null, null, "Could not reach Chapa.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Chapa initialize rejected invoice {InvoiceId} ({Status}): {Body}", invoiceId, response.StatusCode, raw);
            return new InitiatePaymentResult(false, null, null, "Chapa did not accept the payment request.");
        }

        var parsed = JsonSerializer.Deserialize<ChapaInitializeResponse>(raw, JsonOptions);
        if (parsed?.Data?.CheckoutUrl is not { Length: > 0 } checkoutUrl)
        {
            _logger.LogError("Chapa initialize response had no checkout_url for invoice {InvoiceId}: {Body}", invoiceId, raw);
            return new InitiatePaymentResult(false, null, null, "Chapa's response didn't include a checkout link.");
        }

        return new InitiatePaymentResult(true, checkoutUrl, txRef, null);
    }

    public async Task<PaymentCallbackResult> ParseCallbackAsync(string rawPayload, IDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        headers.TryGetValue("x-chapa-signature", out var payloadSignature);
        headers.TryGetValue("chapa-signature", out var secretSignature);

        var payloadSignatureValid = HmacSignatureValidator.Validate(rawPayload, payloadSignature, _credential.Secret);
        var secretSignatureValid = !string.IsNullOrEmpty(_credential.Secret)
            && HmacSignatureValidator.Validate(_credential.Secret, secretSignature, _credential.Secret);

        if (!payloadSignatureValid && !secretSignatureValid)
        {
            _logger.LogWarning("Chapa webhook: signature validation failed");
            return new PaymentCallbackResult(false, null, 0, null, rawPayload, "Signature validation failed.");
        }

        ChapaWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<ChapaWebhookPayload>(rawPayload, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Chapa webhook: malformed JSON payload");
            return new PaymentCallbackResult(false, null, 0, null, rawPayload, "Malformed payload.");
        }

        if (payload?.TxRef is not { Length: > 0 } txRef || !Guid.TryParse(txRef, out var invoiceId))
        {
            _logger.LogWarning("Chapa webhook: tx_ref missing or not a recognizable invoice ID ({TxRef})", payload?.TxRef);
            return new PaymentCallbackResult(false, null, 0, payload?.TxRef, rawPayload, "Unrecognized transaction reference.");
        }

        // Chapa's own guidance: don't trust the webhook body's amount/status
        // alone - re-verify server-to-server before recording a payment.
        var verified = await VerifyAsync(txRef, cancellationToken);
        if (verified is not { Status: "success" })
        {
            _logger.LogWarning("Chapa webhook: verify endpoint did not confirm success for {TxRef} (status: {Status})", txRef, verified?.Status ?? "unreachable");
            return new PaymentCallbackResult(false, invoiceId, 0, txRef, rawPayload, "Payment not confirmed by Chapa's verify endpoint.");
        }

        return new PaymentCallbackResult(true, invoiceId, verified.Amount, txRef, rawPayload, null);
    }

    private async Task<ChapaVerifyData?> VerifyAsync(string txRef, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ChapaVerifyResponse>($"transaction/verify/{Uri.EscapeDataString(txRef)}", JsonOptions, cancellationToken);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chapa verify call failed for {TxRef}", txRef);
            return null;
        }
    }

    private static string? BuildUrl(string? baseUrl, string path)
        => string.IsNullOrEmpty(baseUrl) ? null : $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

    private class ChapaInitializeResponse
    {
        [JsonPropertyName("data")] public ChapaInitializeData? Data { get; set; }
    }

    private class ChapaInitializeData
    {
        [JsonPropertyName("checkout_url")] public string? CheckoutUrl { get; set; }
    }

    private class ChapaWebhookPayload
    {
        [JsonPropertyName("tx_ref")] public string? TxRef { get; set; }
    }

    private class ChapaVerifyResponse
    {
        [JsonPropertyName("data")] public ChapaVerifyData? Data { get; set; }
    }

    private class ChapaVerifyData
    {
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("amount")] public decimal Amount { get; set; }
    }
}
