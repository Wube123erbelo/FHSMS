using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Application.Payments.Commands.RecordPayment;
using FHSMS.Application.Receipts.Queries.GetReceiptById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class PaymentsController : ApiControllerBase
{
    private readonly IReceiptVerifierRegistry _verifierRegistry;

    public PaymentsController(IReceiptVerifierRegistry verifierRegistry)
    {
        _verifierRegistry = verifierRegistry;
    }

    /// <summary>Which provider keys support automatic "I already paid" verification right now (see IReceiptVerifier) - drives which options the frontend offers before falling back to manual recording.</summary>
    [HttpGet("verifiers")]
    public IActionResult GetVerifiers() => Ok(_verifierRegistry.AvailableProviderKeys);

    /// <summary>
    /// Records a payment and immediately issues its receipt. Real payment-provider
    /// callbacks (Telebirr, Chapa, CBE Birr) belong in Infrastructure-level webhook
    /// endpoints that validate the provider's signature and then call this same
    /// command - see PaymentWebhooksController. Safe to retry: sending the same
    /// Reference twice returns the original payment/receipt instead of duplicating it.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RecordPaymentResult>> Record(RecordPaymentCommand command)
        => Ok(await Mediator.Send(command));

    [HttpGet("receipts/{receiptId:guid}")]
    public async Task<ActionResult<ReceiptDto>> GetReceipt(Guid receiptId)
        => Ok(await Mediator.Send(new GetReceiptByIdQuery(receiptId)));
}
