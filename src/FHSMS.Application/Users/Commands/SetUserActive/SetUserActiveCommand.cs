using MediatR;

namespace FHSMS.Application.Users.Commands.SetUserActive;

/// <summary>Suspend (false) or reactivate (true) a user account. A suspended user can no longer log in.</summary>
public record SetUserActiveCommand(Guid UserId, bool IsActive) : IRequest;
