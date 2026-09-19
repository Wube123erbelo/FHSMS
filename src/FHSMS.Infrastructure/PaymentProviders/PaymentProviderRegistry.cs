using FHSMS.Application.Common.Interfaces;

namespace FHSMS.Infrastructure.PaymentProviders;

/// <summary>
/// Resolves a provider by a URL-friendly key (e.g. "telebirr", "chapa") so
/// PaymentWebhooksController has exactly one action method instead of one per
/// provider. Adding provider #6 means: write a class implementing
/// IPaymentProvider, register it in DependencyInjection.cs - this registry
/// and the controller are already generic over however many are registered.
/// </summary>
public class PaymentProviderRegistry : IPaymentProviderRegistry
{
    private readonly Dictionary<string, IPaymentProvider> _providers;

    public PaymentProviderRegistry(IEnumerable<IPaymentProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Method.ToString().ToLowerInvariant(), p => p);
    }

    public IReadOnlyCollection<string> AvailableProviderKeys => _providers.Keys.ToList();

    public IPaymentProvider Resolve(string providerKey)
    {
        var key = providerKey.ToLowerInvariant();
        if (!_providers.TryGetValue(key, out var provider))
            throw new KeyNotFoundException($"No payment provider registered for key '{providerKey}'. Available: {string.Join(", ", AvailableProviderKeys)}");

        return provider;
    }
}
