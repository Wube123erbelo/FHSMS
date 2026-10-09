using FHSMS.Domain.Common;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A single-use refresh token. Rotation policy: every successful refresh
/// issues a brand new token and immediately revokes the one just used
/// (ReplacedByTokenId links them for audit) - a refresh token can never be
/// used twice. If a revoked/used token is presented again, that's a signal
/// of token theft (see AuthController's refresh handling), not just an
/// expired-token error.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public string? CreatedByIp { get; private set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    private RefreshToken() { } // EF Core

    public RefreshToken(Guid userId, string token, TimeSpan validFor, string? createdByIp)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = DateTime.UtcNow.Add(validFor);
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = createdByIp;
    }

    public void Revoke(Guid? replacedByTokenId = null)
    {
        if (RevokedAt is not null)
            throw new DomainException("This refresh token has already been revoked.");
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
