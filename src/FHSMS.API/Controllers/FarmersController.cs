using FHSMS.API.Common;
using FHSMS.Application.Common.Models;
using FHSMS.Application.Farmers.Commands.CreateFarmer;
using FHSMS.Application.Farmers.Commands.SetFarmerActive;
using FHSMS.Application.Farmers.Commands.UpdateFarmer;
using FHSMS.Application.Farmers.Queries.GetFarmers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize(Roles = "SuperAdmin,FarmerAgent")]
public class FarmersController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FarmerDto>>> GetAll()
        => Ok(await Mediator.Send(new GetFarmersQuery()));

    [HttpGet("export/csv")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportCsv()
    {
        var farmers = await Mediator.Send(new GetFarmersQuery());
        var csv = CsvExporter.Write(farmers, Columns());
        return File(csv, "text/csv", "farmers.csv");
    }

    [HttpGet("export/excel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportExcel()
    {
        var farmers = await Mediator.Send(new GetFarmersQuery());
        var xls = ExcelExporter.Write("Farmers", farmers, Columns());
        return File(xls, "application/vnd.ms-excel", "farmers.xls");
    }

    [HttpGet("export/pdf")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportPdf()
    {
        var farmers = await Mediator.Send(new GetFarmersQuery());
        var pdf = PdfTableExporter.Write("Farmers", farmers, Columns());
        return File(pdf, "application/pdf", "farmers.pdf");
    }

    private static (string, Func<FarmerDto, object?>)[] Columns() => new (string, Func<FarmerDto, object?>)[]
    {
        ("Code", f => f.Code),
        ("Name", f => f.Name),
        ("Contact person", f => f.ContactPerson),
        ("Phone", f => f.Phone),
        ("Location", f => f.Location),
        ("Active", f => f.IsActive)
    };

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateFarmerCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPut("{farmerId:guid}")]
    public async Task<IActionResult> Update(Guid farmerId, UpdateFarmerRequest request)
    {
        await Mediator.Send(new UpdateFarmerCommand(
            farmerId, request.Name, request.ContactPerson, request.Phone, request.Location, request.BankAccountNumber, request.IsActive));
        return NoContent();
    }

    [HttpPost("{farmerId:guid}/suspend")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Suspend(Guid farmerId)
    {
        await Mediator.Send(new SetFarmerActiveCommand(farmerId, false));
        return NoContent();
    }

    [HttpPost("{farmerId:guid}/activate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Activate(Guid farmerId)
    {
        await Mediator.Send(new SetFarmerActiveCommand(farmerId, true));
        return NoContent();
    }

    public record UpdateFarmerRequest(
        string Name, string? ContactPerson, string? Phone, string? Location, string? BankAccountNumber, bool IsActive);
}
