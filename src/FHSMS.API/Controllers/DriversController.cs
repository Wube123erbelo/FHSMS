using FHSMS.Application.Common.Models;
using FHSMS.Application.Drivers.Commands.AcceptTrip;
using FHSMS.Application.Drivers.Commands.RegisterDriverProfile;
using FHSMS.Application.Drivers.Queries.GetAllDrivers;
using FHSMS.Application.Drivers.Queries.GetAvailableTrips;
using FHSMS.Application.Drivers.Queries.GetMyDriverEarnings;
using FHSMS.Application.Drivers.Queries.GetMyDriverProfile;
using FHSMS.Application.Drivers.Queries.GetMyTrips;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// The Driver Portal - registered truck drivers manage their own profile and
/// the open trip board. Restricted to the Driver role (plus SuperAdmin, who
/// can see everything for support/testing) - a hotel or farmer agent has no
/// business here, matching the client's "restricted access to their
/// respective entry forms only" requirement.
///
/// Admins do NOT register drivers here - that would let an admin account
/// create unlimited driver profiles under itself, duplicating the "one
/// login, one driver profile" rule. Admins create driver LOGIN accounts via
/// the Users page (role = Driver) exactly like any other agent, and the
/// driver fills in their own profile after logging in. GetAllDrivers below
/// is the admin-side "follow-up" view - read-only monitoring, not a second
/// registration path.
/// </summary>
[Authorize(Roles = "SuperAdmin,Driver")]
public class DriversController : ApiControllerBase
{
    [HttpGet("profile/me")]
    public async Task<ActionResult<DriverDto?>> GetMyProfile()
        => Ok(await Mediator.Send(new GetMyDriverProfileQuery()));

    /// <summary>Creates the caller's driver profile on first use, or updates it on subsequent calls - the "Truck Driver Registration" form.</summary>
    [HttpPost("profile")]
    public async Task<ActionResult<Guid>> RegisterProfile(RegisterDriverProfileCommand command)
        => Ok(await Mediator.Send(command));

    /// <summary>The "Available Trips" board - every open, unassigned delivery, plus any admin has booked this driver for directly (unconfirmed).</summary>
    [HttpGet("trips/available")]
    public async Task<ActionResult<List<TripDto>>> GetAvailableTrips()
        => Ok(await Mediator.Send(new GetAvailableTripsQuery()));

    /// <summary>Trips the caller has accepted or been assigned - the "My Trips" tab.</summary>
    [HttpGet("trips/mine")]
    public async Task<ActionResult<List<TripDto>>> GetMyTrips()
        => Ok(await Mediator.Send(new GetMyTripsQuery()));

    /// <summary>Claims an open trip, or confirms one an admin assigned directly - same endpoint handles both (see AcceptTripCommandHandler).</summary>
    [HttpPost("trips/{deliveryId:guid}/accept")]
    public async Task<IActionResult> AcceptTrip(Guid deliveryId)
    {
        await Mediator.Send(new AcceptTripCommand(deliveryId));
        return NoContent();
    }

    /// <summary>The driver's own earnings - "income for picking products" on their dashboard.</summary>
    [HttpGet("earnings/me")]
    public async Task<ActionResult<DriverEarningsDto>> GetMyEarnings()
        => Ok(await Mediator.Send(new GetMyDriverEarningsQuery()));

    /// <summary>Admin's read-only "Drivers" follow-up list - every registered driver with their trip stats. Not a registration screen.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<AdminDriverDto>>> GetAllDrivers()
        => Ok(await Mediator.Send(new GetAllDriversQuery()));
}
