using FHSMS.Application.Reports.Queries.GetPublicStats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>Anonymous, read-only endpoints for the public marketing site (landing page) - no auth required.</summary>
[AllowAnonymous]
public class PublicController : ApiControllerBase
{
    /// <summary>Backs the landing page's "What We've Achieved So Far" band with real counts instead of hardcoded copy.</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<PublicStatsDto>> Stats()
        => Ok(await Mediator.Send(new GetPublicStatsQuery()));
}
