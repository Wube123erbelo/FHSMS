using FHSMS.Application.BankReconciliation.Commands.ReconcileTransaction;
using FHSMS.Application.BankReconciliation.Commands.RecordBankTransaction;
using FHSMS.Application.BankReconciliation.Queries.GetUnreconciledTransactions;
using FHSMS.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
[Route("api/bank-reconciliation")]
public class BankReconciliationController : ApiControllerBase
{
    public record ReconcileRequest(Guid PaymentId);

    [HttpPost("transactions")]
    public async Task<ActionResult<Guid>> RecordTransaction(RecordBankTransactionCommand command)
        => Ok(await Mediator.Send(command));

    [HttpGet("transactions/unreconciled")]
    public async Task<ActionResult<List<BankTransactionDto>>> GetUnreconciled([FromQuery] Guid? bankAccountId)
        => Ok(await Mediator.Send(new GetUnreconciledTransactionsQuery(bankAccountId)));

    [HttpPost("transactions/{bankTransactionId:guid}/reconcile")]
    public async Task<IActionResult> Reconcile(Guid bankTransactionId, [FromBody] ReconcileRequest request)
    {
        await Mediator.Send(new ReconcileTransactionCommand(bankTransactionId, request.PaymentId));
        return NoContent();
    }

    [HttpPost("transactions/{bankTransactionId:guid}/dispute")]
    public async Task<IActionResult> Dispute(Guid bankTransactionId)
    {
        await Mediator.Send(new MarkTransactionDisputedCommand(bankTransactionId));
        return NoContent();
    }
}
