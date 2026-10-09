using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Auth.Commands.Register;

/// <summary>
/// Public self-registration. Deliberately restricted to customer/portal roles -
/// SuperAdmin, HotelAgent, and FarmerAgent accounts must be created by an
/// existing admin via POST /api/users (see Users.Commands.CreateUser), never
/// through this anonymous endpoint. Without this check, anyone could register
/// themselves as SuperAdmin.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
{
    private static readonly UserRole[] SelfRegisterableRoles =
    {
        UserRole.HotelCustomer,
        UserRole.PublicPortalUser
    };

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDocumentNumberGenerator _numberGenerator;

    public RegisterCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IDocumentNumberGenerator numberGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (!SelfRegisterableRoles.Contains(request.Role))
            throw new FHSMS.Application.Common.Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.Role), "This role cannot be self-registered. Ask an administrator to create staff/agent accounts.")
            });

        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (exists)
            throw new FHSMS.Application.Common.Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "Email is already registered.")
            });

        var user = new User
        {
            Code = await _numberGenerator.NextAgentCodeAsync(request.Role, cancellationToken),
            FullName = request.FullName,
            Email = request.Email,
            Role = request.Role
        };
        user.SetPasswordHash(_passwordHasher.Hash(request.Password));

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
