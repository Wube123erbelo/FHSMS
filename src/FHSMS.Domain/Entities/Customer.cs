using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

/// <summary>A hotel customer (buyer). Farmers are modelled separately as supply-side agents (see Farmer).</summary>
public class Customer : AuditableEntity
{
    /// <summary>Human-readable sequential code (e.g. "HTL-0001") - the "hotel ID" shown in dropdowns instead of a raw GUID.</summary>
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Maximum outstanding balance this customer is allowed to carry across
    /// unpaid invoices. Null means no limit is enforced. Actual exposure is
    /// computed on demand (sum of Invoice.BalanceDue), not stored here, so it
    /// can never drift out of sync with the invoices it's derived from.
    /// </summary>
    public decimal? CreditLimit { get; set; }
}
