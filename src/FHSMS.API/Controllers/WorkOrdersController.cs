using FHSMS.Application.Common.Models;
using FHSMS.Application.WorkOrders.Commands.CreateWorkOrder;
using FHSMS.Application.WorkOrders.Commands.UpdateWorkOrderStatus;
using FHSMS.Application.WorkOrders.Queries.GetWorkOrders;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>Internal operational tasks - dispatch prep, farm pickups, restock checks, etc.</summary>
[Authorize]
public class WorkOrdersController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<WorkOrderDto>>> GetAll([FromQuery] Guid? assignedToUserId, [FromQuery] WorkOrderStatus? status)
        => Ok(await Mediator.Send(new GetWorkOrdersQuery(assignedToUserId, status)));

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HotelAgent,FarmerAgent")]
    public async Task<ActionResult<Guid>> Create(CreateWorkOrderCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPost("{workOrderId:guid}/start")]
    public async Task<IActionResult> Start(Guid workOrderId)
    {
        await Mediator.Send(new StartWorkOrderCommand(workOrderId));
        return NoContent();
    }

    [HttpPost("{workOrderId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid workOrderId)
    {
        await Mediator.Send(new CompleteWorkOrderCommand(workOrderId));
        return NoContent();
    }

    [HttpPost("{workOrderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid workOrderId)
    {
        await Mediator.Send(new CancelWorkOrderCommand(workOrderId));
        return NoContent();
    }

    [HttpPost("{workOrderId:guid}/reassign")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Reassign(Guid workOrderId, [FromBody] ReassignRequest request)
    {
        await Mediator.Send(new ReassignWorkOrderCommand(workOrderId, request.AssignedToUserId));
        return NoContent();
    }

    public record ReassignRequest(Guid? AssignedToUserId);
}
