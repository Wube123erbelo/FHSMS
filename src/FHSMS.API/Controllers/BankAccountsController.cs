using FHSMS.Application.BankAccounts.Commands.CreateBankAccount;
using FHSMS.Application.BankAccounts.Commands.DeleteBankAccount;
using FHSMS.Application.BankAccounts.Commands.UpdateBankAccount;
using FHSMS.Application.BankAccounts.Queries.GetBankAccounts;
using FHSMS.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// GetAll is intentionally readable by any authenticated role - "which
/// bank accounts can I transfer to" is information a hotel agent or
/// customer needs before paying, not an admin-only secret. Create/Update/
/// Delete stay SuperAdmin-only since those actually change where money is
/// expected to land.
/// </summary>
[Authorize]
public class BankAccountsController : ApiControllerBase
{
    public record UpdateBankAccountRequest(string BankName, string AccountName, string AccountNumber, bool IsActive);

    [HttpGet]
    public async Task<ActionResult<List<BankAccountDto>>> GetAll()
        => Ok(await Mediator.Send(new GetBankAccountsQuery()));

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<Guid>> Create(CreateBankAccountCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPut("{bankAccountId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(Guid bankAccountId, UpdateBankAccountRequest request)
    {
        await Mediator.Send(new UpdateBankAccountCommand(bankAccountId, request.BankName, request.AccountName, request.AccountNumber, request.IsActive));
        return NoContent();
    }

    [HttpDelete("{bankAccountId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid bankAccountId)
    {
        await Mediator.Send(new DeleteBankAccountCommand(bankAccountId));
        return NoContent();
    }
}
