using FHSMS.API.Common;
using FHSMS.Application.AuditLogs.Queries.GetAuditLogs;
using FHSMS.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class AuditLogsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuditLogDto>>> Get(
        [FromQuery] string? entityName, [FromQuery] Guid? entityId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        => Ok(await Mediator.Send(new GetAuditLogsQuery(entityName, entityId, page, pageSize)));

    // Exports pull a large single page rather than a separate unpaginated
    // query - simplest way to reuse the exact same filtering/ordering logic
    // the on-screen table already uses.

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] string? entityName, [FromQuery] Guid? entityId)
    {
        var logs = await Mediator.Send(new GetAuditLogsQuery(entityName, entityId, 1, 10000));
        var csv = CsvExporter.Write(logs, Columns());
        return File(csv, "text/csv", "audit-logs.csv");
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] string? entityName, [FromQuery] Guid? entityId)
    {
        var logs = await Mediator.Send(new GetAuditLogsQuery(entityName, entityId, 1, 10000));
        var xls = ExcelExporter.Write("Audit Logs", logs, Columns());
        return File(xls, "application/vnd.ms-excel", "audit-logs.xls");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] string? entityName, [FromQuery] Guid? entityId)
    {
        var logs = await Mediator.Send(new GetAuditLogsQuery(entityName, entityId, 1, 10000));
        var pdf = PdfTableExporter.Write("Audit Logs", logs, Columns());
        return File(pdf, "application/pdf", "audit-logs.pdf");
    }

    private static (string, Func<AuditLogDto, object?>)[] Columns() => new (string, Func<AuditLogDto, object?>)[]
    {
        ("Entity", l => l.EntityName),
        ("Action", l => l.Action),
        ("Performed by", l => l.PerformedBy),
        ("Performed at", l => l.PerformedAt)
    };
}
