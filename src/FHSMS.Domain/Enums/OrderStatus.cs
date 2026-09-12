namespace FHSMS.Domain.Enums;

/// <summary>
/// Order state machine exactly as specified: Draft -> Pending -> Confirmed ->
/// Preparing -> Shipped -> Delivered -> Completed, with Rejected/Cancelled/
/// Returned as the exception paths. Allowed transitions are enforced in
/// Order's domain methods (Submit/Confirm/Reject/Prepare/Ship/Deliver/
/// Complete/Return/Cancel), not scattered through application handlers.
/// </summary>
public enum OrderStatus
{
    Draft = 1,
    Pending = 2,
    Confirmed = 3,
    Preparing = 4,
    Shipped = 5,
    Delivered = 6,
    Completed = 7,
    Rejected = 8,
    Cancelled = 9,
    Returned = 10
}
