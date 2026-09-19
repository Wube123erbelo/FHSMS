using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Payments.Events;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Payments.Commands.DeclarePayment;

public class DeclarePaymentCommandHandler : IRequestHandler<DeclarePaymentCommand, DeclarePaymentResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublisher _publisher;

    public DeclarePaymentCommandHandler(
        IApplicationDbContext context,
        IDocumentNumberGenerator numberGenerator,
        ICurrentUserService currentUser,
        IPublisher publisher)
    {
        _context = context;
        _numberGenerator = numberGenerator;
        _currentUser = currentUser;
        _publisher = publisher;
    }

    public async Task<DeclarePaymentResult> Handle(DeclarePaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.InvoiceId), "This invoice has been cancelled.")
            });

        if (invoice.BalanceDue <= 0)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.InvoiceId), "This invoice is already fully paid.")
            });

        if (request.Amount > invoice.BalanceDue)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Amount),
                    $"That's more than the {invoice.BalanceDue:N2} ETB still outstanding on this invoice.")
            });

        // Reference is uniquely indexed on Payments, so catch a re-submission
        // here and answer it cleanly instead of letting the database throw. A
        // customer tapping "confirm" twice should see their existing claim, not
        // an error.
        var existing = await _context.Payments
            .FirstOrDefaultAsync(p => p.Reference == request.Reference, cancellationToken);

        if (existing is { IsPending: true } && existing.InvoiceId == invoice.Id)
            return new DeclarePaymentResult(
                existing.Id, existing.PaymentNumber, existing.Amount, existing.Reference!, existing.Status.ToString());

        if (existing is not null)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Reference),
                    "That reference has already been submitted. If you think this is a mistake, contact support.")
            });

        var paymentNumber = await _numberGenerator.NextPaymentNumberAsync(cancellationToken);
        var claim = Payment.CreateClaim(
            paymentNumber, invoice.Id, request.Amount, request.Method, request.Reference, request.BankAccountId);

        _context.Payments.Add(claim);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new PaymentClaimDeclaredEvent(
                claim.Id, invoice.Id, invoice.InvoiceNumber, claim.Amount, claim.PaymentNumber,
                request.Reference, _currentUser.UserId, _currentUser.Email),
            cancellationToken);

        return new DeclarePaymentResult(
            claim.Id, claim.PaymentNumber, claim.Amount, request.Reference, claim.Status.ToString());
    }
}
