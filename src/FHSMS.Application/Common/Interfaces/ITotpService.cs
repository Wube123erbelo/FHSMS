namespace FHSMS.Application.Common.Interfaces;

/// <summary>TOTP (RFC 6238) generation/validation for two-factor authentication, and secret provisioning.</summary>
public interface ITotpService
{
    /// <summary>Generates a new random Base32-encoded secret suitable for an authenticator app.</summary>
    string GenerateSecret();

    /// <summary>Builds the otpauth:// URI an authenticator app's QR scanner expects.</summary>
    string BuildOtpAuthUri(string secret, string accountEmail, string issuer);

    /// <summary>Validates a 6-digit code against the secret, tolerating +/- one 30-second time step for clock drift.</summary>
    bool ValidateCode(string secret, string code);
}
