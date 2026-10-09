namespace FHSMS.Application.Common.Models;

public class CustomerCreditStatusDto
{
    public Guid CustomerId { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal CurrentExposure { get; set; }
    public decimal? AvailableCredit { get; set; }
    public bool OverLimit { get; set; }
    public int OutstandingInvoiceCount { get; set; }
}
