using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mail;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using FHSMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FHSMS.Infrastructure.Services;

/// <summary>
/// Default notification implementation: persists the notification, then
/// tries to actually deliver it.
///
/// Email sends for real via SmtpClient once EmailSettings.Enabled is true
/// and SmtpHost/FromAddress are configured - System.Net.Mail is in-box in
/// .NET (no extra package to restore), which is why it's used here over a
/// richer library like MailKit; swap it out if this needs OAuth2 or other
/// features SmtpClient doesn't support.
///
/// Sms sends for real via a generic HTTP POST once SmsSettings.Enabled is
/// true and ApiUrl is configured - see SmsSettings' remarks on why that
/// request shape is a best-effort template rather than a guaranteed match
/// for whichever gateway gets wired in.
///
/// Telegram is still a logging stub - no Telegram Bot API push is wired up.
/// Either way, everything upstream only ever talks to INotificationService,
/// so plugging in a real Telegram sender later never touches
/// Application-layer code.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly SmsSettings _smsSettings;
    private readonly HttpClient _http;

    public NotificationService(
        ApplicationDbContext context,
        ILogger<NotificationService> logger,
        IOptions<EmailSettings> emailSettings,
        IOptions<SmsSettings> smsSettings,
        HttpClient http)
    {
        _context = context;
        _logger = logger;
        _emailSettings = emailSettings.Value;
        _smsSettings = smsSettings.Value;
        _http = http;
    }

    public async Task QueueAsync(
        Guid? recipientUserId,
        string? recipientAddress,
        NotificationChannel channel,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        // Most handlers only know a phone/email, not a User id (the recipient
        // might not even have a login - e.g. a Customer). Resolve one here
        // whenever possible so the in-app notifications list (which is keyed on
        // RecipientUserId, see GetMyNotificationsQueryHandler) isn't silently
        // limited to the handful of call sites that already pass a user id
        // explicitly.
        var resolvedUserId = recipientUserId ?? await ResolveUserIdAsync(recipientAddress, cancellationToken);

        var notification = new Domain.Entities.Notification(resolvedUserId, recipientAddress, channel, subject, body);
        _context.Notifications.Add(notification);

        try
        {
            await DispatchAsync(channel, recipientAddress, subject, body, cancellationToken);
            notification.MarkSent();
        }
        catch (Exception ex)
        {
            notification.MarkFailed(ex.Message);
            _logger.LogWarning(ex, "Failed to dispatch {Channel} notification to {Recipient}", channel, recipientAddress);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid?> ResolveUserIdAsync(string? recipientAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipientAddress))
            return null;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == recipientAddress || u.Phone == recipientAddress, cancellationToken);

        return user?.Id;
    }

    private async Task DispatchAsync(NotificationChannel channel, string? recipientAddress, string subject, string body, CancellationToken cancellationToken)
    {
        if (channel == NotificationChannel.Email && _emailSettings.Enabled)
        {
            await SendEmailAsync(recipientAddress, subject, body, cancellationToken);
            return;
        }

        if (channel == NotificationChannel.Sms && _smsSettings.Enabled)
        {
            await SendSmsAsync(recipientAddress, body, cancellationToken);
            return;
        }

        // Email/Sms with sending disabled or unconfigured, or Telegram (no
        // provider integrated yet) - log so the flow stays visible
        // end-to-end during development instead of silently vanishing.
        _logger.LogInformation(
            "[STUB {Channel} SEND] To: {Recipient} | Subject: {Subject} | Body: {Body}",
            channel, recipientAddress ?? "(no address)", subject, body);
    }

    private async Task SendEmailAsync(string? recipientAddress, string subject, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipientAddress))
            throw new InvalidOperationException("Cannot send an email notification without a recipient address.");

        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpHost) || string.IsNullOrWhiteSpace(_emailSettings.FromAddress))
            throw new InvalidOperationException("Email:Enabled is true but Email:SmtpHost/FromAddress aren't configured.");

        using var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(recipientAddress);

        using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
        {
            EnableSsl = _emailSettings.EnableSsl
        };
        if (!string.IsNullOrEmpty(_emailSettings.SmtpUsername))
            client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

        await client.SendMailAsync(message, cancellationToken);
    }

    private async Task SendSmsAsync(string? recipientPhone, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipientPhone))
            throw new InvalidOperationException("Cannot send an SMS notification without a recipient phone number.");

        if (string.IsNullOrWhiteSpace(_smsSettings.ApiUrl))
            throw new InvalidOperationException("Sms:Enabled is true but Sms:ApiUrl isn't configured.");

        // Generic best-effort shape - see SmsSettings' remarks. Confirm
        // field names/auth style against the actual gateway before relying
        // on this in production.
        var payload = new { to = recipientPhone, message = body, senderId = _smsSettings.SenderId };
        using var request = new HttpRequestMessage(HttpMethod.Post, _smsSettings.ApiUrl)
        {
            Content = JsonContent.Create(payload)
        };
        if (!string.IsNullOrEmpty(_smsSettings.ApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _smsSettings.ApiKey);

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
