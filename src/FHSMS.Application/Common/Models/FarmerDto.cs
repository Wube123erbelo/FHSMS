namespace FHSMS.Application.Common.Models;

public class FarmerDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? BankAccountNumber { get; set; }
    public bool IsActive { get; set; }
}
