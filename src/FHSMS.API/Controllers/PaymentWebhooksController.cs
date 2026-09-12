using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Payments.Commands.RecordPayment;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// One generic webhook endpoint for every payment provider - "any bank,
/// Telebirr, Chapa, CBE Birr, and future providers" per the payment/banking
/// architecture requirement. Adding a new provider means writing a class that
/// implements IPaymentProvider and registering it in
/// FHSMS.Infrastructure.DependencyInjection - this controller, and the
/// business rule of what a payment does to an invoice, never change.
///
/// POST /api/webhooks/payments/{providerKey}  e.g. /telebirr, /chapa, /cbebirr, /bank
/// </summary>
[AllowAnonymous]
[Route("api/webhooks/payments")]
public class PaymentWebhooksController : ApiControllerBase
{
    private readonly IPaymentProviderRegistry _providerRegistry;

    public PaymentWebhooksController(IPaymentProviderRegistry providerRegistry)
    {
        _providerRegistry = providerRegistry;
    }

    [HttpPost("{providerKey}")]
    public async Task<IActionResult> Handle(string providerKey, CancellationToken cancellationToken)
    {
        IPaymentProvider provider;
        try
        {
            provider = _providerRegistry.Resolve(providerKey);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawPayload = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        var result = await provider.ParseCallbackAsync(rawPayload, headers, cancellationToken);

        if (!result.IsValid || result.InvoiceId is null)
        {
            // Deliberately 400, not 401/403 - don't help an attacker distinguish
            // "bad signature" from "bad payload shape" from the response alone.
            return BadRequest(new { message = result.FailureReason ?? "Invalid payment callback." });
        }

        var payment = await Mediator.Send(new RecordPaymentCommand(
            result.InvoiceId.Value,
            result.Amount,
            provider.Method,
            result.ProviderReference,
            BankAccountId: null,
            ProviderResponse: result.RawPayload));

        return Ok(payment);
    }

    /// <summary>Lists which provider keys are currently registered - useful when configuring a new provider's webhook URL.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult ListProviders() => Ok(_providerRegistry.AvailableProviderKeys);
}
