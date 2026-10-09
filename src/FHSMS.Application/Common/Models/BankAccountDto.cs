namespace FHSMS.Application.Common.Models;

public class BankAccountDto
{
    public Guid Id { get; set; }
    public string BankName { get; set; } = default!;
    public string AccountName { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public bool IsActive { get; set; }
}
