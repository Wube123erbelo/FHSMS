using FHSMS.Application.Common.Models;
using FHSMS.Application.Units.Commands.CreateUnit;
using FHSMS.Application.Units.Commands.DeleteUnit;
using FHSMS.Application.Units.Commands.UpdateUnit;
using FHSMS.Application.Units.Queries.GetUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class UnitsController : ApiControllerBase
{
    public record UpdateUnitRequest(string Name, string Abbreviation, bool IsActive);

    [HttpGet]
    public async Task<ActionResult<List<UnitDto>>> GetAll()
        => Ok(await Mediator.Send(new GetUnitsQuery()));

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<Guid>> Create(CreateUnitCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPut("{unitId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(Guid unitId, UpdateUnitRequest request)
    {
        await Mediator.Send(new UpdateUnitCommand(unitId, request.Name, request.Abbreviation, request.IsActive));
        return NoContent();
    }

    [HttpDelete("{unitId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid unitId)
    {
        await Mediator.Send(new DeleteUnitCommand(unitId));
        return NoContent();
    }
}
