using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Drivers.Commands.RegisterDriverProfile;

/// <summary>
/// Called by a logged-in Driver-role user to create or update their own
/// profile - this is the "Truck Driver Registration" form in the Driver
/// Portal mockup. UserId always comes from the authenticated caller
/// (ICurrentUserService in the handler), never from the request body, so a
/// driver can only ever create/edit their own profile.
/// </summary>
public record RegisterDriverProfileCommand(
    string FullName, string? Phone, string? PlateNumber, TruckType TruckType) : IRequest<Guid>;
