namespace FHSMS.Application.Common.Interfaces;

/// <summary>Generates cryptographically random opaque refresh token strings (not JWTs - just random bytes, base64url-encoded).</summary>
public interface IRefreshTokenGenerator
{
    string Generate();
}
