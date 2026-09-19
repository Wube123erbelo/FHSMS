namespace FHSMS.Infrastructure.Services;

/// <summary>
/// Configuration for an optional SMS gateway. Off by default, same shape as
/// EmailSettings - nothing changes for a deployment that never fills this
/// in, NotificationService just keeps logging a stub for the Sms channel.
///
/// There's no single standard SMS API the way there's a standard SMTP
/// protocol, and Ethiopian gateways (AfroMessage, GeezSMS, etc.) each define
/// their own request shape. NotificationService.SendSmsAsync posts a generic
/// { to, message, senderId } JSON body with "Authorization: Bearer
/// {ApiKey}" - a reasonable starting point, but treat it as a template to
/// adjust against whichever gateway's actual docs you're integrating, not a
/// guarantee it matches. Nothing outside NotificationService needs to
/// change either way - the rest of the app only ever calls
/// INotificationService.QueueAsync.
/// </summary>
public class SmsSettings
{
    public const string SectionName = "Sms";

    /// <summary>Master switch. Leave false until ApiUrl/ApiKey are confirmed against a real gateway.</summary>
    public bool Enabled { get; set; } = false;
    /// <summary>Full URL of the gateway's "send" endpoint.</summary>
    public string? ApiUrl { get; set; }
    /// <summary>Sent as "Authorization: Bearer {ApiKey}". Change SendSmsAsync if your gateway instead expects an API key as a query parameter or a different header.</summary>
    public string? ApiKey { get; set; }
    /// <summary>Optional sender ID/short code some gateways require.</summary>
    public string? SenderId { get; set; }
}
