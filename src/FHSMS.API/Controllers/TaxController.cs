using FHSMS.Application.Common.Models;
using FHSMS.Application.Tax.Commands.ConfigureTax;
using FHSMS.Application.Tax.Commands.UpdateVatToggle;
using FHSMS.Application.Tax.Queries.GetActiveTaxConfiguration;
using FHSMS.Application.Tax.Queries.GetTaxHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// Backs the Settings -> Tax &amp; VAT admin screen. Every write here is the ONLY
/// way tax rates/behaviour enter the system - there is no other code path that
/// can introduce a rate.
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class TaxController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous] // reading current config is needed by order/invoice screens too
    public async Task<ActionResult<List<TaxConfigurationDto>>> GetActive()
        => Ok(await Mediator.Send(new GetActiveTaxConfigurationsQuery()));

    [HttpGet("{taxConfigurationId:guid}/history")]
    public async Task<ActionResult<List<TaxRateHistoryDto>>> GetHistory(Guid taxConfigurationId)
        => Ok(await Mediator.Send(new GetTaxRateHistoryQuery(taxConfigurationId)));

    [HttpPost("configure")]
    public async Task<ActionResult<Guid>> Configure(ConfigureTaxCommand command)
        => Ok(await Mediator.Send(command));

    public record ToggleTaxRequest(bool IsEnabled);

    [HttpPost("{taxConfigurationId:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid taxConfigurationId, [FromBody] ToggleTaxRequest request)
    {
        await Mediator.Send(new SetTaxEnabledCommand(taxConfigurationId, request.IsEnabled));
        return NoContent();
    }
}
