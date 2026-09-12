using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Drivers.Queries.GetAvailableTrips;

/// <summary>The "Available Trips" board - every Pending delivery no driver has accepted yet.</summary>
public record GetAvailableTripsQuery : IRequest<List<TripDto>>;
