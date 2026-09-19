using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Payments.Commands.InitiateInvoicePayment;

public class InitiateInvoicePaymentCommandHandler : MediatR.IRequestHandler<InitiateInvoicePaymentCommand, InitiateInvoicePaymentResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentProviderRegistry _providerRegistry;

    public InitiateInvoicePaymentCommandHandler(IApplicationDbContext context, IPaymentProviderRegistry providerRegistry)
    {
        _context = context;
        _providerRegistry = providerRegistry;
    }

    public async Task<InitiateInvoicePaymentResult> Handle(InitiateInvoicePaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Invoice), request.InvoiceId);

        if (invoice.BalanceDue <= 0)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.InvoiceId), "This invoice is already fully paid.")
            });

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == invoice.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Customer), invoice.CustomerId);

        if (string.IsNullOrWhiteSpace(customer.Email))
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(customer.Email), "This customer has no email on file - most hosted checkouts require one to send a payment link to.")
            });

        IPaymentProvider provider;
        try
        {
            provider = _providerRegistry.Resolve(request.ProviderKey);
        }
        catch (KeyNotFoundException ex)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ProviderKey), ex.Message)
            });
        }

        if (provider is not IInitiatablePaymentProvider initiatable)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ProviderKey),
                    $"{request.ProviderKey} doesn't support starting a payment from here yet - it can only receive a payment confirmation once one's been made another way.")
            });

        var result = await initiatable.InitiateAsync(invoice.Id, invoice.BalanceDue, customer.Email, customer.Name, cancellationToken);

        if (!result.Success || result.CheckoutUrl is null)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ProviderKey), result.FailureReason ?? "Could not start a payment with this provider.")
            });

        return new InitiateInvoicePaymentResult(result.CheckoutUrl);
    }
}
