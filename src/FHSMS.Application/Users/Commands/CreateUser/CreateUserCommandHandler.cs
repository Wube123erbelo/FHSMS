using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    // Mirrors RegisterCommandHandler.SelfRegisterableRoles from the other
    // side: those two roles are only ever created via the public /auth/register
    // endpoint (a hotel customer or portal shopper signing themselves up),
    // never by an admin here - keeps there being exactly one place each kind
    // of account gets created, instead of two paths that can drift apart.
    private static readonly UserRole[] SelfRegisterableRoles =
    {
        UserRole.HotelCustomer,
        UserRole.PublicPortalUser
    };

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDocumentNumberGenerator _numberGenerator;

    public CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IDocumentNumberGenerator numberGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (SelfRegisterableRoles.Contains(request.Role))
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.Role), "Hotel customers and public portal users sign themselves up - they aren't created from here.")
            });

        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (exists)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "Email is already registered.")
            });

        var user = new User
        {
            Code = await _numberGenerator.NextAgentCodeAsync(request.Role, cancellationToken),
            FullName = request.FullName,
            Email = request.Email,
            Role = request.Role,
            Phone = request.Phone,
            Location = request.Location
        };
        user.SetPasswordHash(_passwordHasher.Hash(request.Password));

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
