using FHSMS.API.Common;
using FHSMS.Application.Common.Models;
using FHSMS.Application.Customers.Commands.CreateCustomer;
using FHSMS.Application.Customers.Commands.DeleteCustomer;
using FHSMS.Application.Customers.Commands.SetCustomerActive;
using FHSMS.Application.Customers.Commands.UpdateCustomer;
using FHSMS.Application.Customers.Queries.GetCustomerCreditStatus;
using FHSMS.Application.Customers.Queries.GetCustomers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class CustomersController : ApiControllerBase
{
    public record UpdateCustomerRequest(
        string Name, string? ContactPerson, string? Phone, string? Email, string? Address, decimal? CreditLimit, bool IsActive);

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
        => Ok(await Mediator.Send(new GetCustomersQuery()));

    [HttpGet("export/csv")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportCsv()
    {
        var customers = await Mediator.Send(new GetCustomersQuery());
        var csv = CsvExporter.Write(customers, new (string, Func<CustomerDto, object?>)[]
        {
            ("Name", c => c.Name),
            ("Contact person", c => c.ContactPerson),
            ("Phone", c => c.Phone),
            ("Email", c => c.Email),
            ("Address", c => c.Address),
            ("Credit limit", c => c.CreditLimit),
            ("Active", c => c.IsActive)
        });
        return File(csv, "text/csv", "customers.csv");
    }

    [HttpGet("export/excel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportExcel()
    {
        var customers = await Mediator.Send(new GetCustomersQuery());
        var xls = ExcelExporter.Write("Customers", customers, new (string, Func<CustomerDto, object?>)[]
        {
            ("Name", c => c.Name),
            ("Contact person", c => c.ContactPerson),
            ("Phone", c => c.Phone),
            ("Email", c => c.Email),
            ("Address", c => c.Address),
            ("Credit limit", c => c.CreditLimit),
            ("Active", c => c.IsActive)
        });
        return File(xls, "application/vnd.ms-excel", "customers.xls");
    }

    [HttpGet("export/pdf")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportPdf()
    {
        var customers = await Mediator.Send(new GetCustomersQuery());
        var pdf = PdfTableExporter.Write("Customers", customers, new (string, Func<CustomerDto, object?>)[]
        {
            ("Name", c => c.Name),
            ("Contact person", c => c.ContactPerson),
            ("Phone", c => c.Phone),
            ("Credit limit", c => c.CreditLimit),
            ("Active", c => c.IsActive)
        });
        return File(pdf, "application/pdf", "customers.pdf");
    }

    [HttpGet("{customerId:guid}/credit-status")]
    [Authorize(Roles = "SuperAdmin,HotelAgent")]
    public async Task<ActionResult<CustomerCreditStatusDto>> GetCreditStatus(Guid customerId)
        => Ok(await Mediator.Send(new GetCustomerCreditStatusQuery(customerId)));

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCustomerCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPut("{customerId:guid}")]
    [Authorize(Roles = "SuperAdmin,HotelAgent")]
    public async Task<IActionResult> Update(Guid customerId, UpdateCustomerRequest request)
    {
        await Mediator.Send(new UpdateCustomerCommand(
            customerId, request.Name, request.ContactPerson, request.Phone, request.Email, request.Address,
            request.CreditLimit, request.IsActive));
        return NoContent();
    }

    /// <summary>Soft-delete: deactivates the customer. Orders/invoices already placed are untouched.</summary>
    [HttpDelete("{customerId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid customerId)
    {
        await Mediator.Send(new DeleteCustomerCommand(customerId));
        return NoContent();
    }

    [HttpPost("{customerId:guid}/suspend")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Suspend(Guid customerId)
    {
        await Mediator.Send(new SetCustomerActiveCommand(customerId, false));
        return NoContent();
    }

    [HttpPost("{customerId:guid}/activate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Activate(Guid customerId)
    {
        await Mediator.Send(new SetCustomerActiveCommand(customerId, true));
        return NoContent();
    }
}
