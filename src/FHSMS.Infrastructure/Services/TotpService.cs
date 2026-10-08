using System.Security.Cryptography;
using System.Text;
using FHSMS.Application.Common.Interfaces;

namespace FHSMS.Infrastructure.Services;

/// <summary>
/// Hand-rolled TOTP per RFC 6238 (HMAC-SHA1, 30-second step, 6 digits) - the
/// same algorithm Google Authenticator, Authy, and every standard
/// authenticator app implement, so no external TOTP package is needed.
/// </summary>
public class TotpService : ITotpService
{
    private const int TimeStepSeconds = 30;
    private const int Digits = 6;

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20); // 160 bits, standard TOTP secret size
        return Base32Encode(bytes);
    }

    public string BuildOtpAuthUri(string secret, string accountEmail, string issuer)
    {
        var label = Uri.EscapeDataString($"{issuer}:{accountEmail}");
        var issuerParam = Uri.EscapeDataString(issuer);
        return $"otpauth://totp/{label}?secret={secret}&issuer={issuerParam}&algorithm=SHA1&digits={Digits}&period={TimeStepSeconds}";
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != Digits || !code.All(char.IsDigit))
            return false;

        var keyBytes = Base32Decode(secret);
        var currentStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TimeStepSeconds;

        // Accept the current step and one step on either side to tolerate
        // normal clock drift between the server and the user's phone.
        for (var offset = -1; offset <= 1; offset++)
        {
            var candidate = ComputeCode(keyBytes, currentStep + offset);
            if (candidate == code)
                return true;
        }

        return false;
    }

    private static string ComputeCode(byte[] key, long counter)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);

        var offset = hash[^1] & 0x0F;
        var binaryCode =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        var code = binaryCode % (int)Math.Pow(10, Digits);
        return code.ToString().PadLeft(Digits, '0');
    }

    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    private static string Base32Encode(byte[] data)
    {
        var sb = new StringBuilder();
        int bits = 0, value = 0;

        foreach (var b in data)
        {
            value = (value << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                sb.Append(Base32Alphabet[(value >> (bits - 5)) & 0x1F]);
                bits -= 5;
            }
        }

        if (bits > 0)
            sb.Append(Base32Alphabet[(value << (5 - bits)) & 0x1F]);

        return sb.ToString();
    }

    private static byte[] Base32Decode(string base32)
    {
        base32 = base32.TrimEnd('=').ToUpperInvariant();
        var bytes = new List<byte>();
        int bits = 0, value = 0;

        foreach (var c in base32)
        {
            var index = Base32Alphabet.IndexOf(c);
            if (index < 0) continue; // skip any non-alphabet characters defensively
            value = (value << 5) | index;
            bits += 5;
            if (bits >= 8)
            {
                bytes.Add((byte)((value >> (bits - 8)) & 0xFF));
                bits -= 8;
            }
        }

        return bytes.ToArray();
    }
}
