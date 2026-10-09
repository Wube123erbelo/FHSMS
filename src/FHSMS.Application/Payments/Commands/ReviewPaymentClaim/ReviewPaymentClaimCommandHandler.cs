using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Payments.Commands.RecordPayment;
using FHSMS.Application.Payments.Events;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Payments.Commands.ReviewPaymentClaim;

public class ReviewPaymentClaimCommandHandler : IRequestHandler<ReviewPaymentClaimCommand, ReviewPaymentClaimResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IPublisher _publisher;

    public ReviewPaymentClaimCommandHandler(IApplicationDbContext context, IMediator mediator, IPublisher publisher)
    {
        _context = context;
        _mediator = mediator;
        _publisher = publisher;
    }

    public async Task<ReviewPaymentClaimResult> Handle(ReviewPaymentClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), request.PaymentId);

        if (!claim.IsPending)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.PaymentId),
                    $"This claim has already been reviewed - it's {claim.Status}.")
            });

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == claim.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), claim.InvoiceId);

        var amount = request.ConfirmedAmount ?? claim.Amount;

        if (!request.Approve)
        {
            claim.MarkFailed();
            await _context.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(
                new PaymentClaimReviewedEvent(
                    claim.Id, invoice.Id, invoice.InvoiceNumber, claim.Amount, claim.PaymentNumber,
                    Approved: false, request.Note, claim.CreatedBy),
                cancellationToken);

            return new ReviewPaymentClaimResult(claim.Id, claim.PaymentNumber, claim.Status.ToString(), null, null);
        }

        if (amount > invoice.BalanceDue)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ConfirmedAmount),
                    $"That's more than the {invoice.BalanceDue:N2} ETB still outstanding on invoice {invoice.InvoiceNumber}.")
            });

        // Same reference as the pending claim, so RecordPaymentCommandHandler
        // confirms this very row in place rather than inserting a second one -
        // see its remarks. Everything downstream (receipt, PaymentRecordedEvent,
        // customer notification) then behaves exactly as it does for a payment
        // an admin recorded by hand.
        var recorded = await _mediator.Send(
            new RecordPaymentCommand(claim.InvoiceId, amount, claim.Method, claim.Reference, claim.BankAccountId),
            cancellationToken);

        await _publisher.Publish(
            new PaymentClaimReviewedEvent(
                claim.Id, invoice.Id, invoice.InvoiceNumber, amount, claim.PaymentNumber,
                Approved: true, request.Note, claim.CreatedBy),
            cancellationToken);

        return new ReviewPaymentClaimResult(
            recorded.PaymentId, recorded.PaymentNumber, "Completed", recorded.ReceiptId, recorded.ReceiptNumber);
    }
}
