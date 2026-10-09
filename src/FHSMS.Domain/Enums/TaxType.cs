namespace FHSMS.Domain.Enums;

/// <summary>
/// The category of tax a TaxConfiguration represents. The tax engine treats
/// all of these uniformly - none of them are hard-coded as "the" tax.
/// </summary>
public enum TaxType
{
    Vat = 1,
    SalesTax = 2,
    WithholdingTax = 3,
    Other = 99
}
