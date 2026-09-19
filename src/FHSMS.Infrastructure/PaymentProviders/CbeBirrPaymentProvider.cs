using FHSMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FHSMS.Infrastructure.PaymentProviders;

/// <summary>CBE Birr webhook provider - see TelebirrPaymentProvider's remarks; same extension pattern applies.</summary>
public class CbeBirrPaymentProvider : PaymentProviderBase
{
    public CbeBirrPaymentProvider(IOptions<PaymentProviderSettings> settings, ILogger<CbeBirrPaymentProvider> logger)
        : base(settings.Value.CbeBirr, logger)
    {
    }

    public override PaymentMethod Method => PaymentMethod.CbeBirr;
}
