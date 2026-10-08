using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Notifications.Commands.MarkNotificationRead;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.NotificationId);

        if (notification.RecipientUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("You can only mark your own notifications as read.");

        notification.MarkRead();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
