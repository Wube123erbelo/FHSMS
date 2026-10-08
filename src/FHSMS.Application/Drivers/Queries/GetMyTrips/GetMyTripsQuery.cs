using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Drivers.Queries.GetMyTrips;

/// <summary>Trips the calling driver has accepted (any status) - the Driver Portal's "My Trips" tab.</summary>
public record GetMyTripsQuery : IRequest<List<TripDto>>;
