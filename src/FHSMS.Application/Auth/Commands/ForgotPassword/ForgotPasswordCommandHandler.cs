using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(30);

    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public ForgotPasswordCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

        if (user is null)
            return; // deliberately silent - see class remarks

        var token = Guid.NewGuid().ToString("N");
        user.IssuePasswordResetToken(token, TokenLifetime);
        await _context.SaveChangesAsync(cancellationToken);

        // In a real deployment this would be a link into the frontend's reset-password
        // page (e.g. https://app.example.com/reset-password?token=...). The token is
        // included directly in the notification body here since NotificationService
        // is currently a logging stub - see FHSMS.Infrastructure.Services.NotificationService.
        await _notificationService.QueueAsync(
            recipientUserId: user.Id,
            recipientAddress: user.Email,
            channel: NotificationChannel.Email,
            subject: "Reset your FHSMS password",
            body: $"Use this token to reset your password (valid for 30 minutes): {token}",
            cancellationToken: cancellationToken);
    }
}
