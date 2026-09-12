namespace FHSMS.Domain.Enums;

/// <summary>Which channel the order originated from - all channels share the same backend logic.</summary>
public enum OrderSourceType
{
    HotelPortal = 1,
    HotelAgent = 2,
    FarmerAgent = 3,
    Telegram = 4,
    PublicPortal = 5
}
