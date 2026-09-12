using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Drivers.Queries.GetMyDriverProfile;

/// <summary>Returns the calling driver's own profile, or null if they haven't filled it in yet.</summary>
public record GetMyDriverProfileQuery : IRequest<DriverDto?>;
