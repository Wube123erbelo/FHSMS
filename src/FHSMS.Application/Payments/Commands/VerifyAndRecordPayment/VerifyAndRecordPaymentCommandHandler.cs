using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Payments.Commands.VerifyAndRecordPayment;

public class VerifyAndRecordPaymentCommandHandler : IRequestHandler<VerifyAndRecordPaymentCommand, VerifyAndRecordPaymentResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IReceiptVerifierRegistry _verifierRegistry;
    private readonly IMediator _mediator;

    public VerifyAndRecordPaymentCommandHandler(IApplicationDbContext context, IReceiptVerifierRegistry verifierRegistry, IMediator mediator)
    {
        _context = context;
        _verifierRegistry = verifierRegistry;
        _mediator = mediator;
    }

    public async Task<VerifyAndRecordPaymentResult> Handle(VerifyAndRecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Invoice), request.InvoiceId);

        if (invoice.BalanceDue <= 0)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.InvoiceId), "This invoice is already fully paid.")
            });

        var verifier = _verifierRegistry.Resolve(request.ProviderKey)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ProviderKey),
                    $"No automatic verification is available for '{request.ProviderKey}' yet - record this payment manually instead and an admin will reconcile it.")
            });

        var result = await verifier.VerifyAsync(request.Reference, request.SecondaryIdentifier, invoice.BalanceDue, cancellationToken);

        if (!result.IsValid)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Reference), result.FailureReason ?? "Could not verify this payment.")
            });

        var method = request.ProviderKey.ToLowerInvariant() switch
        {
            "cbebirr" => PaymentMethod.CbeBirr,
            "cbe" or "abyssinia" or "awash" or "dashen" => PaymentMethod.Bank,
            "telebirr" => PaymentMethod.Telebirr,
            _ => PaymentMethod.Bank
        };

        var recorded = await _mediator.Send(
            new Payments.Commands.RecordPayment.RecordPaymentCommand(
                invoice.Id, result.Amount, method, result.ProviderReference ?? request.Reference,
                request.BankAccountId, result.RawPayload),
            cancellationToken);

        return new VerifyAndRecordPaymentResult(recorded.PaymentId, recorded.PaymentNumber, recorded.ReceiptId, recorded.ReceiptNumber);
    }
}
