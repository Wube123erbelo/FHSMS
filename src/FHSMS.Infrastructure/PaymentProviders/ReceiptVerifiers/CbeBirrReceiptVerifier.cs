using System.Text.RegularExpressions;
using FHSMS.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FHSMS.Infrastructure.PaymentProviders.ReceiptVerifiers;

/// <summary>
/// CBE Birr (Commercial Bank of Ethiopia's mobile wallet, distinct from
/// Telebirr) publishes a public, unauthenticated HTML receipt page for every
/// completed transaction: GET https://cbepay1.cbe.com.et/aureceipt?TID={reference}&amp;PH={payerPhone}
/// This is the same URL CBE Birr itself generates when a customer taps
/// "share receipt" in their app, and is the standard way small Ethiopian
/// merchants (without a formal CBE merchant agreement) confirm a CBE Birr
/// payment was really made - documented consistently across multiple
/// independent third-party verification tools.
///
/// Caveat: like several Ethiopian bank endpoints, this one may only be
/// reliably reachable from an Ethiopian IP address depending on hosting
/// region - if this provider can't reach it, VerifyAsync returns a clear
/// "couldn't reach CBE Birr" failure reason (not a false "payment not
/// found") so the caller can fall back to manual payment recording instead.
/// </summary>
public class CbeBirrReceiptVerifier : IReceiptVerifier
{
    private readonly HttpClient _http;
    private readonly ILogger<CbeBirrReceiptVerifier> _logger;

    public CbeBirrReceiptVerifier(HttpClient http, ILogger<CbeBirrReceiptVerifier> logger)
    {
        _http = http;
        _logger = logger;
    }

    public string ProviderKey => "cbebirr";

    public async Task<PaymentCallbackResult> VerifyAsync(string reference, string? secondaryIdentifier, decimal expectedAmount, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return Fail(reference, "A CBE Birr transaction reference is required.");
        if (string.IsNullOrWhiteSpace(secondaryIdentifier))
            return Fail(reference, "The payer's CBE Birr phone number is required to look up this receipt.");

        var url = $"https://cbepay1.cbe.com.et/aureceipt?TID={Uri.EscapeDataString(reference)}&PH={Uri.EscapeDataString(secondaryIdentifier)}";

        string html;
        try
        {
            html = await _http.GetStringAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CBE Birr receipt lookup failed for {Reference} - endpoint may be unreachable from this host", reference);
            return Fail(reference, "Couldn't reach CBE Birr's verification service right now. You can still record this payment manually and an admin will reconcile it.");
        }

        // The receipt page is a plain HTML table, not a JSON API - pull the
        // fields we need out with simple, tolerant regexes rather than a
        // full HTML parser dependency. If CBE changes the page's markup
        // this will need updating; that's a real risk with any
        // undocumented public page like this one.
        var amountText = ExtractField(html, "Transferred Amount", "Amount");
        var statusText = ExtractField(html, "Status");

        if (amountText is null)
        {
            _logger.LogWarning("CBE Birr receipt for {Reference} didn't contain a recognizable amount field - page markup may have changed", reference);
            return Fail(reference, "Couldn't read this CBE Birr receipt - either the reference is wrong or CBE changed their receipt page format.");
        }

        if (!TryParseAmount(amountText, out var amount))
            return Fail(reference, $"Couldn't parse the amount on this receipt ('{amountText}').");

        if (statusText is not null && !statusText.Contains("success", StringComparison.OrdinalIgnoreCase)
            && !statusText.Contains("complete", StringComparison.OrdinalIgnoreCase))
            return Fail(reference, $"This CBE Birr transaction isn't marked as completed (status: {statusText}).");

        if (amount < expectedAmount)
            return Fail(reference, $"This receipt is for {amount:0.00} ETB, which is less than the {expectedAmount:0.00} ETB owed on this invoice.");

        return new PaymentCallbackResult(true, null, amount, reference, html, null);
    }

    private static PaymentCallbackResult Fail(string reference, string reason)
        => new(false, null, 0, reference, "", reason);

    private static string? ExtractField(string html, params string[] labels)
    {
        foreach (var label in labels)
        {
            // Matches a table-row-ish "Label ... value" pattern loosely -
            // tolerant of the exact tag soup around it.
            var match = Regex.Match(html, $@"{Regex.Escape(label)}\s*[:<>/a-zA-Z]*\s*([\d,]+\.?\d*)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;
        }
        return null;
    }

    private static bool TryParseAmount(string text, out decimal amount)
        => decimal.TryParse(text.Replace(",", "").Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out amount);
}
