using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

public class BankAccount : AuditableEntity
{
    public string BankName { get; set; } = default!;
    public string AccountName { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}
