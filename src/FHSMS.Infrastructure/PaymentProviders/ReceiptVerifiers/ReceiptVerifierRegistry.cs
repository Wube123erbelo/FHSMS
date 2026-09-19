using FHSMS.Application.Common.Interfaces;

namespace FHSMS.Infrastructure.PaymentProviders.ReceiptVerifiers;

public class ReceiptVerifierRegistry : IReceiptVerifierRegistry
{
    private readonly Dictionary<string, IReceiptVerifier> _verifiers;

    public ReceiptVerifierRegistry(IEnumerable<IReceiptVerifier> verifiers)
    {
        _verifiers = verifiers.ToDictionary(v => v.ProviderKey.ToLowerInvariant(), v => v);
    }

    public IReadOnlyCollection<string> AvailableProviderKeys => _verifiers.Keys.ToList();

    public IReceiptVerifier? Resolve(string providerKey)
        => _verifiers.TryGetValue(providerKey.ToLowerInvariant(), out var v) ? v : null;
}
