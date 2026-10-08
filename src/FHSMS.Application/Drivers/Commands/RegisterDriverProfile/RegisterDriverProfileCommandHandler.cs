using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Drivers.Commands.RegisterDriverProfile;

public class RegisterDriverProfileCommandHandler : IRequestHandler<RegisterDriverProfileCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDocumentNumberGenerator _numberGenerator;

    public RegisterDriverProfileCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, IDocumentNumberGenerator numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(RegisterDriverProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Must be logged in to register a driver profile.");

        var existing = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (existing is not null)
        {
            existing.FullName = request.FullName;
            existing.Phone = request.Phone;
            existing.PlateNumber = request.PlateNumber;
            existing.TruckType = request.TruckType;
            await _context.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var driver = new Domain.Entities.Driver
        {
            Code = await _numberGenerator.NextDriverCodeAsync(cancellationToken),
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            PlateNumber = request.PlateNumber,
            TruckType = request.TruckType
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync(cancellationToken);
        return driver.Id;
    }
}
