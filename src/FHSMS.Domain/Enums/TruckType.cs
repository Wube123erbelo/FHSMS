namespace FHSMS.Domain.Enums;

/// <summary>The kind of vehicle a registered driver operates - shown on the trip board so a dispatcher/admin can match load size to truck.</summary>
public enum TruckType
{
    Pickup = 1,
    SmallTruck = 2,
    MediumTruck = 3,
    Isuzu = 4,
    HeavyTruck = 5,
    Trailer = 6,
    Other = 7
}
