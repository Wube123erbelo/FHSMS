using FHSMS.Application.Common.Models;
using FHSMS.Application.PlatformCommission.Commands.ConfigurePlatformCommission;
using FHSMS.Application.PlatformCommission.Queries.GetPlatformCommissionConfiguration;
using FHSMS.Application.PlatformCommission.Queries.GetPlatformCommissionHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// Backs the Settings -> Total Commission admin screen. Every write here is
/// the ONLY way the platform commission rate enters the system - the 2% the
/// company charges hotels on every sale is never hard-coded anywhere else,
/// and every new invoice picks up whatever is configured here at the moment
/// it's generated.
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class PlatformCommissionController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous] // reading the current rate is needed by order/invoice screens too, same as Tax
    public async Task<ActionResult<PlatformCommissionConfigurationDto?>> GetActive()
        => Ok(await Mediator.Send(new GetPlatformCommissionConfigurationQuery()));

    [HttpGet("{configurationId:guid}/history")]
    public async Task<ActionResult<List<PlatformCommissionRateHistoryDto>>> GetHistory(Guid configurationId)
        => Ok(await Mediator.Send(new GetPlatformCommissionHistoryQuery(configurationId)));

    [HttpPost("configure")]
    public async Task<ActionResult<Guid>> Configure(ConfigurePlatformCommissionCommand command)
        => Ok(await Mediator.Send(command));
}
