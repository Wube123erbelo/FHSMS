using FHSMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FHSMS.Infrastructure.PaymentProviders;

/// <summary>
/// Generic bank-transfer confirmation webhook - covers any bank that can
/// notify FHSMS of an incoming transfer via HTTP callback, not tied to one
/// specific bank. Which physical BankAccount the money landed in is
/// reconciled separately (see BankReconciliationController) rather than
/// assumed from the webhook alone.
/// </summary>
public class BankTransferPaymentProvider : PaymentProviderBase
{
    public BankTransferPaymentProvider(IOptions<PaymentProviderSettings> settings, ILogger<BankTransferPaymentProvider> logger)
        : base(settings.Value.BankTransfer, logger)
    {
    }

    public override PaymentMethod Method => PaymentMethod.Bank;
}
