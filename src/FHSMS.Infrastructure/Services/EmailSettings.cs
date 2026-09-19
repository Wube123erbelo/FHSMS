namespace FHSMS.Infrastructure.Services;

/// <summary>
/// SMTP configuration for real outbound email. Mirrors the same
/// "off by default until configured" shape PaymentProviderSettings uses -
/// Enabled defaults to false so a deployment with an empty Email section
/// keeps behaving exactly like before (NotificationService logs a stub
/// instead of trying to send), rather than throwing on every notification
/// the moment this ships. Real deployments set these via user-secrets/
/// environment variables, never committed to appsettings.json.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>Master switch. Leave false in any environment without real SMTP credentials.</summary>
    public bool Enabled { get; set; } = false;
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    /// <summary>STARTTLS - true for virtually every provider (SendGrid, Mailgun, Gmail SMTP relay, Office365) on the standard 587 port.</summary>
    public bool EnableSsl { get; set; } = true;
    public string? FromAddress { get; set; }
    public string FromName { get; set; } = "FHSMS";
}
