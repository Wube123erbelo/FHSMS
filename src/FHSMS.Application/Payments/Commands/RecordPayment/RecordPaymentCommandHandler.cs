using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Payments.Events;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Payments.Commands.RecordPayment;

/// <summary>
/// Records a payment against an invoice and immediately issues its Receipt.
/// Payment providers (any bank, Telebirr, Chapa, CBE Birr, future ones) are
/// expected to call into this same command after their own webhook/callback
/// validation happens in Infrastructure via IPaymentProvider - the business
/// rule of "how does a payment affect an invoice" lives in exactly one place
/// regardless of which of N providers triggered it.
///
/// Idempotency: if a Reference is supplied and a completed payment with that
/// exact reference already exists, this returns the existing payment/receipt
/// instead of posting a duplicate - a retried webhook (which providers do
/// routinely) can never double-credit an invoice.
/// </summary>
public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, RecordPaymentResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublisher _publisher;

    public RecordPaymentCommandHandler(
        IApplicationDbContext context, IDocumentNumberGenerator numberGenerator, ICurrentUserService currentUser, IPublisher publisher)
    {
        _context = context;
        _numberGenerator = numberGenerator;
        _currentUser = currentUser;
        _publisher = publisher;
    }

    public async Task<RecordPaymentResult> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        if (!string.IsNullOrWhiteSpace(request.Reference))
        {
            var existing = await _context.Payments
                .FirstOrDefaultAsync(p => p.Reference == request.Reference, cancellationToken);

            // A pending self-declared claim (see Payment.CreateClaim) carries the
            // same reference but has never touched the invoice and has no receipt.
            // Confirming it is what this call means, so settle it in place rather
            // than inserting a second row - Reference is uniquely indexed, and the
            // claim and the payment it becomes are the same record.
            if (existing is { IsPending: true })
            {
                existing.Confirm(request.Amount);
                invoice.RegisterPayment(existing.Amount);

                var claimReceiptNumber = await _numberGenerator.NextReceiptNumberAsync(cancellationToken);
                var claimReceipt = new Receipt(
                    claimReceiptNumber, existing.Id, invoice.Id, existing.Amount,
                    existing.Method.ToString(), existing.Reference, _currentUser.Email);
                _context.Receipts.Add(claimReceipt);

                await _context.SaveChangesAsync(cancellationToken);

                await _publisher.Publish(
                    new PaymentRecordedEvent(existing.Id, invoice.Id, invoice.CustomerId, existing.Amount, existing.PaymentNumber, claimReceipt.ReceiptNumber),
                    cancellationToken);

                return new RecordPaymentResult(existing.Id, existing.PaymentNumber, claimReceipt.Id, claimReceipt.ReceiptNumber);
            }

            if (existing is not null)
            {
                var existingReceipt = await _context.Receipts
                    .FirstOrDefaultAsync(r => r.PaymentId == existing.Id, cancellationToken);

                if (existingReceipt is null)
                    throw new ValidationException(new[]
                    {
                        new FluentValidation.Results.ValidationFailure(nameof(request.Reference),
                            $"Reference '{request.Reference}' belongs to a {existing.Status} payment and can't be recorded again.")
                    });

                // Deliberately no PaymentRecordedEvent here - see the class
                // remarks on RecordPaymentCommandHandler: a retried webhook
                // must not re-notify the customer for a payment they
                // already got a receipt for.
                return new RecordPaymentResult(existing.Id, existing.PaymentNumber, existingReceipt.Id, existingReceipt.ReceiptNumber);
            }
        }

        var paymentNumber = await _numberGenerator.NextPaymentNumberAsync(cancellationToken);
        var payment = new Payment(
            paymentNumber, invoice.Id, request.Amount, request.Method, request.Reference,
            request.BankAccountId, request.ProviderResponse);
        invoice.RegisterPayment(request.Amount);
        _context.Payments.Add(payment);

        // A receipt is only ever generated after a payment is recorded as
        // completed - never on its own, and never for a failed/pending payment.
        var receiptNumber = await _numberGenerator.NextReceiptNumberAsync(cancellationToken);
        var receipt = new Receipt(
            receiptNumber, payment.Id, invoice.Id, payment.Amount, payment.Method.ToString(), payment.Reference, _currentUser.Email);
        _context.Receipts.Add(receipt);

        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new PaymentRecordedEvent(payment.Id, invoice.Id, invoice.CustomerId, payment.Amount, payment.PaymentNumber, receipt.ReceiptNumber),
            cancellationToken);

        return new RecordPaymentResult(payment.Id, payment.PaymentNumber, receipt.Id, receipt.ReceiptNumber);
    }
}
