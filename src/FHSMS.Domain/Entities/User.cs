using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

public class User : AuditableEntity
{
    /// <summary>Human-readable sequential code (e.g. "HAGT-0001", "FAGT-0001", "ADM-0001") - the "agent ID" shown in listings instead of a raw GUID.</summary>
    public string? Code { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Phone { get; set; }
    public string? Location { get; set; }
    /// <summary>Updated on every authenticated request (see LastSeenMiddleware) - not just login, so "online" reflects actual recent activity, not just whether they signed in at some point today.</summary>
    public DateTime? LastSeenAt { get; set; }

    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    /// <summary>
    /// Base32 TOTP secret. Present as soon as setup is initiated but
    /// TwoFactorEnabled stays false until the user confirms one valid code -
    /// an abandoned setup never silently locks the account out.
    /// </summary>
    public string? TwoFactorSecret { get; private set; }
    public bool TwoFactorEnabled { get; private set; }

    public User() { } // EF Core + object initializer usage elsewhere

    public void SetPasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash cannot be empty.");
        PasswordHash = hash;
    }

    /// <summary>Issues a one-time reset token valid for a limited window. Any previous token is overwritten.</summary>
    public void IssuePasswordResetToken(string token, TimeSpan validFor)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = DateTime.UtcNow.Add(validFor);
    }

    public bool IsPasswordResetTokenValid(string token) =>
        PasswordResetToken is not null
        && PasswordResetTokenExpiresAt is not null
        && PasswordResetTokenExpiresAt > DateTime.UtcNow
        && PasswordResetToken == token;

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
    }

    public void BeginTwoFactorSetup(string secret)
    {
        TwoFactorSecret = secret;
        TwoFactorEnabled = false;
    }

    public void ConfirmTwoFactor()
    {
        if (TwoFactorSecret is null)
            throw new DomainException("Two-factor setup was never started for this account.");
        TwoFactorEnabled = true;
    }

    public void DisableTwoFactor()
    {
        TwoFactorSecret = null;
        TwoFactorEnabled = false;
    }
}
