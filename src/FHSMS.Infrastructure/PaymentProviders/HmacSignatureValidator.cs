using System.Security.Cryptography;
using System.Text;

namespace FHSMS.Infrastructure.PaymentProviders;

/// <summary>
/// Shared HMAC-SHA256 signature check used by every provider implementation.
/// Each real provider documents its own exact scheme (which header, which
/// encoding, whether it signs the raw body or a canonicalized string) - swap
/// the body of Validate for the provider's documented algorithm when
/// integrating against real credentials. This class exists so that swap only
/// has to happen in one place per provider, not scattered through handlers.
/// </summary>
public static class HmacSignatureValidator
{
    public static bool Validate(string payload, string? providedSignatureHex, string? secret)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(providedSignatureHex))
            return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedHex = Convert.ToHexString(computed);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHex),
            Encoding.UTF8.GetBytes(providedSignatureHex.ToUpperInvariant()));
    }
}
