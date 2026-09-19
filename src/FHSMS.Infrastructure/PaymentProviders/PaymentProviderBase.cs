using System.Text.Json;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FHSMS.Infrastructure.PaymentProviders;

/// <summary>
/// Shared parsing/validation flow for every provider: deserialize the
/// standard payload shape, verify its signature against the configured
/// secret, and return a normalized PaymentCallbackResult either way (never
/// throws for "invalid payload" - that's a legitimate outcome a webhook
/// caller should get a clean 400 for, not a 500).
/// </summary>
public abstract class PaymentProviderBase : IPaymentProvider
{
    private readonly PaymentProviderSettings.ProviderCredential _credential;
    private readonly ILogger _logger;

    protected PaymentProviderBase(PaymentProviderSettings.ProviderCredential credential, ILogger logger)
    {
        _credential = credential;
        _logger = logger;
    }

    public abstract PaymentMethod Method { get; }

    public Task<PaymentCallbackResult> ParseCallbackAsync(string rawPayload, IDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        WebhookPayloadDto? payload;
        try
        {
            payload = JsonSerializer.Deserialize<WebhookPayloadDto>(rawPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "{Provider} webhook: malformed JSON payload", Method);
            return Task.FromResult(new PaymentCallbackResult(false, null, 0, null, rawPayload, "Malformed payload."));
        }

        if (payload is null)
            return Task.FromResult(new PaymentCallbackResult(false, null, 0, null, rawPayload, "Empty payload."));

        headers.TryGetValue(_credential.SignatureHeader, out var headerSignature);
        var signature = payload.Signature ?? headerSignature;

        if (!HmacSignatureValidator.Validate(rawPayload, signature, _credential.Secret))
        {
            _logger.LogWarning("{Provider} webhook: signature validation failed for invoice {InvoiceId}", Method, payload.InvoiceId);
            return Task.FromResult(new PaymentCallbackResult(false, payload.InvoiceId, payload.Amount, payload.Reference, rawPayload, "Signature validation failed."));
        }

        return Task.FromResult(new PaymentCallbackResult(true, payload.InvoiceId, payload.Amount, payload.Reference, rawPayload, null));
    }
}
