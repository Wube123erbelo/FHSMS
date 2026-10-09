using MediatR;

namespace FHSMS.Domain.Common;

/// <summary>
/// Marker interface for domain events. Domain events are dispatched (via MediatR)
/// after a successful SaveChanges, so infrastructure/application concerns (e.g.
/// "send a Telegram notification when an invoice is issued") never leak into the
/// domain layer itself.
/// </summary>
public interface IDomainEvent : INotification
{
}
