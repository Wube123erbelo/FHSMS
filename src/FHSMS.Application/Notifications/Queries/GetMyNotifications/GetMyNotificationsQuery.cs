using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Notifications.Queries.GetMyNotifications;

/// <summary>
/// The caller's own notifications - no userId parameter, derived from their
/// own token (ICurrentUserService), same principle used everywhere else in
/// this app. The previous version of this query took a raw userId in the
/// URL with no ownership check at all - any authenticated user could read
/// anyone else's notifications by guessing their GUID. This replaces it.
/// </summary>
public record GetMyNotificationsQuery : IRequest<List<NotificationDto>>;
