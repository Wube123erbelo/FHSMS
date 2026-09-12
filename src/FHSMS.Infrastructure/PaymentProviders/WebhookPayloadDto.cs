using System.Text.Json.Serialization;

namespace FHSMS.Infrastructure.PaymentProviders;

/// <summary>
/// The normalized JSON shape every provider implementation here expects on
/// its webhook body: { "invoiceId": "...", "amount": 123.45, "reference": "...", "signature": "..." }.
/// Real providers each have their own actual payload shape - map from the
/// provider's real fields to this DTO inside that provider's ParseCallbackAsync
/// once you have their live documentation/sandbox to test against.
/// </summary>
public class WebhookPayloadDto
{
    [JsonPropertyName("invoiceId")]
    public Guid InvoiceId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}
