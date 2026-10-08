using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Users.Commands.CreateUser;

/// <summary>
/// Admin-only user creation - the only path that can create SuperAdmin,
/// HotelAgent, or FarmerAgent accounts (see RegisterCommandHandler for why
/// the public endpoint refuses those roles).
/// </summary>
public record CreateUserCommand(string FullName, string Email, string Password, UserRole Role, string? Phone = null, string? Location = null) : IRequest<Guid>;
