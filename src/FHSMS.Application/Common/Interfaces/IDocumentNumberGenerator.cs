using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Interfaces;

/// <summary>Generates sequential, human-readable identifiers (order numbers, invoice numbers, ...).</summary>
public interface IDocumentNumberGenerator
{
    Task<string> NextOrderNumberAsync(CancellationToken cancellationToken = default);
    Task<string> NextInvoiceNumberAsync(CancellationToken cancellationToken = default);
    Task<string> NextFarmerInvoiceNumberAsync(CancellationToken cancellationToken = default);
    Task<string> NextPaymentNumberAsync(CancellationToken cancellationToken = default);
    Task<string> NextReceiptNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>Human-readable sequential codes (e.g. "CAT-0001") used wherever the UI shows an ID instead of a raw GUID.</summary>
    Task<string> NextCategoryCodeAsync(CancellationToken cancellationToken = default);
    Task<string> NextUnitCodeAsync(CancellationToken cancellationToken = default);
    Task<string> NextCustomerCodeAsync(CancellationToken cancellationToken = default);
    Task<string> NextFarmerCodeAsync(CancellationToken cancellationToken = default);
    Task<string> NextDriverCodeAsync(CancellationToken cancellationToken = default);
    Task<string> NextAgentCodeAsync(UserRole role, CancellationToken cancellationToken = default);
}
