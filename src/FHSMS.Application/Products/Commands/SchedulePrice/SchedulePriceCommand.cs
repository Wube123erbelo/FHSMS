using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Products.Commands.SchedulePrice;

/// <summary>
/// The ONLY way a product's price changes. Requires a reason, exactly like a
/// real pricing-approval trail would expect - "because the market price
/// changed", "seasonal adjustment", etc. Kind selects which of the two
/// independent timelines (Buying or Selling) this change applies to - the
/// other kind's history/current price is completely untouched.
/// </summary>
public record SchedulePriceCommand(Guid ProductId, PriceKind Kind, decimal Price, DateTime EffectiveFrom, string? Reason) : IRequest<Guid>;
