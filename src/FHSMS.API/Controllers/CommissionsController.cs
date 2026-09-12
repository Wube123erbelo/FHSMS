using FHSMS.API.Common;
using FHSMS.Application.Commissions.Commands.CalculateCommission;
using FHSMS.Application.Commissions.Commands.ConfigureCommissionRule;
using FHSMS.Application.Commissions.Commands.DeleteCommissionRule;
using FHSMS.Application.Commissions.Commands.RemoveCommissionUnitRate;
using FHSMS.Application.Commissions.Commands.SetCommissionUnitRate;
using FHSMS.Application.Commissions.Queries.GetCommissionRules;
using FHSMS.Application.Commissions.Queries.GetCommissionsByAgent;
using FHSMS.Application.Commissions.Queries.GetMyCommissions;
using FHSMS.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class CommissionsController : ApiControllerBase
{
    [HttpGet("rules")]
    public async Task<ActionResult<List<CommissionRuleDto>>> GetRules()
        => Ok(await Mediator.Send(new GetCommissionRulesQuery()));

    /// <summary>The calling agent's own commissions - e.g. the dashboard's "today's earnings" card. No ID needed; derived from the caller's token.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<List<CommissionDto>>> GetMine()
        => Ok(await Mediator.Send(new GetMyCommissionsQuery()));

    /// <summary>
    /// Creates or updates the commission rule for an agent type - basis
    /// (percentage vs. flat-rate-per-quantity) and rate. An admin can call
    /// this at any time; the new rate applies to every commission accrued
    /// afterwards, never retroactively.
    /// </summary>
    [HttpPost("rules")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<Guid>> ConfigureRule(ConfigureCommissionRuleCommand command)
        => Ok(await Mediator.Send(command));

    [HttpDelete("rules/{commissionRuleId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteRule(Guid commissionRuleId)
    {
        await Mediator.Send(new DeleteCommissionRuleCommand(commissionRuleId));
        return NoContent();
    }

    /// <summary>Sets a per-unit rate override on a flat-rate rule (e.g. a different birr amount per kg vs. per quintal). Adjustable at any time.</summary>
    [HttpPut("rules/{commissionRuleId:guid}/unit-rates/{unitId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SetUnitRate(Guid commissionRuleId, Guid unitId, [FromBody] SetCommissionUnitRateBody body)
    {
        await Mediator.Send(new SetCommissionUnitRateCommand(commissionRuleId, unitId, body.RateAmount));
        return NoContent();
    }

    /// <summary>Removes a per-unit override, reverting that unit back to the rule's default rate.</summary>
    [HttpDelete("rules/{commissionRuleId:guid}/unit-rates/{unitId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RemoveUnitRate(Guid commissionRuleId, Guid unitId)
    {
        await Mediator.Send(new RemoveCommissionUnitRateCommand(commissionRuleId, unitId));
        return NoContent();
    }

    [HttpGet("agents/{agentUserId:guid}")]
    public async Task<ActionResult<List<CommissionDto>>> GetByAgent(Guid agentUserId)
        => Ok(await Mediator.Send(new GetCommissionsByAgentQuery(agentUserId)));

    [HttpGet("agents/{agentUserId:guid}/export/csv")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportCsv(Guid agentUserId)
    {
        var commissions = await Mediator.Send(new GetCommissionsByAgentQuery(agentUserId));
        var csv = CsvExporter.Write(commissions, Columns());
        return File(csv, "text/csv", "commissions.csv");
    }

    [HttpGet("agents/{agentUserId:guid}/export/excel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportExcel(Guid agentUserId)
    {
        var commissions = await Mediator.Send(new GetCommissionsByAgentQuery(agentUserId));
        var xls = ExcelExporter.Write("Commissions", commissions, Columns());
        return File(xls, "application/vnd.ms-excel", "commissions.xls");
    }

    [HttpGet("agents/{agentUserId:guid}/export/pdf")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportPdf(Guid agentUserId)
    {
        var commissions = await Mediator.Send(new GetCommissionsByAgentQuery(agentUserId));
        var pdf = PdfTableExporter.Write("Commissions", commissions, Columns());
        return File(pdf, "application/pdf", "commissions.pdf");
    }

    private static (string, Func<CommissionDto, object?>)[] Columns() => new (string, Func<CommissionDto, object?>)[]
    {
        ("Source", c => c.SourceType),
        ("Basis", c => c.Basis),
        ("Base amount", c => c.BaseAmount),
        ("Commission amount", c => c.CommissionAmount),
        ("Status", c => c.Status),
        ("Date", c => c.CreatedAt)
    };

    /// <summary>Manual/retroactive calculation - normally this happens automatically via InvoiceIssuedEvent.</summary>
    [HttpPost("calculate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<Guid?>> Calculate(CalculateCommissionCommand command)
        => Ok(await Mediator.Send(command));
}

public record SetCommissionUnitRateBody(decimal RateAmount);
