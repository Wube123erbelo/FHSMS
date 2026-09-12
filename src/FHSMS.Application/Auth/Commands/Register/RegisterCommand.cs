using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Auth.Commands.Register;

public record RegisterCommand(string FullName, string Email, string Password, UserRole Role) : IRequest<Guid>;
