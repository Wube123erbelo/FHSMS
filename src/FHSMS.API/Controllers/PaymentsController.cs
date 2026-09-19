using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Application.Payments.Commands.RecordPayment;
using FHSMS.Application.Payments.Commands.ReviewPaymentClaim;
using FHSMS.Application.Payments.Queries.GetPaymentClaims;
using FHSMS.Application.Receipts.Queries.GetReceiptById;
using FHSMS.Domain.Enums;
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
    /// Records a payment and immediately issues its receipt - no independent
    /// verification happens here, it's a direct "trust me, this was paid" entry.
    /// SuperAdmin-only for exactly that reason: an agent could otherwise mark
    /// their own invoice paid with no real payment behind it. Real payment-provider
    /// callbacks (Telebirr, Chapa, CBE Birr) belong in Infrastructure-level webhook
    /// endpoints that validate the provider's signature and then call this same
    /// command - see PaymentWebhooksController, which is unaffected by this
    /// restriction since it calls RecordPaymentCommand via Mediator directly,
    /// never through this HTTP endpoint. Safe to retry: sending the same
    /// Reference twice returns the original payment/receipt instead of duplicating it.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<RecordPaymentResult>> Record(RecordPaymentCommand command)
        => Ok(await Mediator.Send(command));

    [HttpGet("receipts/{receiptId:guid}")]
    public async Task<ActionResult<ReceiptDto>> GetReceipt(Guid receiptId)
        => Ok(await Mediator.Send(new GetReceiptByIdQuery(receiptId)));

    /// <summary>
    /// The admin work queue of self-declared "I already paid" claims (see
    /// InvoicesController.DeclarePayment) - defaults to the ones still awaiting
    /// a verdict. Pass ?status=Completed or ?status=Failed to review history.
    /// </summary>
    [HttpGet("claims")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<PaymentClaimDto>>> GetClaims([FromQuery] PaymentStatus? status)
        => Ok(await Mediator.Send(new GetPaymentClaimsQuery(status)));

    public record ReviewClaimRequest(bool Approve, decimal? ConfirmedAmount, string? Note);

    /// <summary>
    /// An admin's verdict on a claim after checking it against the bank
    /// statement: approving settles the invoice and issues a receipt through the
    /// exact same RecordPaymentCommand path a webhook or a manual staff entry
    /// uses; rejecting leaves the invoice balance untouched. SuperAdmin-only for
    /// the same reason POST /payments is - this is the step that actually moves
    /// money.
    /// </summary>
    [HttpPost("claims/{paymentId:guid}/review")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ReviewPaymentClaimResult>> ReviewClaim(Guid paymentId, ReviewClaimRequest request)
        => Ok(await Mediator.Send(new ReviewPaymentClaimCommand(paymentId, request.Approve, request.ConfirmedAmount, request.Note)));
}
