namespace FHSMS.Domain.Enums;

/// <summary>
/// A product now carries two independent, independently-scheduled price
/// tracks instead of one:
///   - Buying: what the company pays a farmer per unit (used to cost stock
///     receipts and pay farmers).
///   - Selling: what a hotel is charged per unit (used to price orders).
/// Both follow the exact same effective-dated, never-overwritten pattern -
/// see ProductPrice and Product.SchedulePrice/GetPriceAsOf.
/// </summary>
public enum PriceKind
{
    Buying = 1,
    Selling = 2
}
