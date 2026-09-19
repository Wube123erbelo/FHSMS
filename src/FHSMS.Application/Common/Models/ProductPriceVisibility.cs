namespace FHSMS.Application.Common.Models;

/// <summary>
/// Single source of truth for which of a product's two prices a given role
/// may see. Applied once, in GetProductsQueryHandler, so every surface that
/// reads the product list - the admin web app, the hotel/farmer agent
/// dashboards, the driver app, the hotel self-service portal, and the
/// Telegram bot (which all authenticate as one of these roles and hit this
/// same endpoint) - gets consistent results without re-implementing the rule.
///
///   SuperAdmin   -> both (admin has two roles: recording transactions needs
///                   buying price, placing an order on a hotel's behalf needs
///                   selling price - the UI auto-selects the right one per
///                   screen, but the API always gives admin both so it can).
///   Driver       -> both (needs to reconcile what was bought from the farmer
///                   against what will be charged to the hotel on delivery).
///   FarmerAgent  -> buying price only (they stock in from farmers at this price).
///   HotelAgent, HotelCustomer, PublicPortalUser -> selling price only
///                   (their entire job is placing/paying for hotel orders).
/// </summary>
public static class ProductPriceVisibility
{
    public static (bool ShowBuying, bool ShowSelling) For(string? role) => role switch
    {
        "SuperAdmin" => (true, true),
        "Driver" => (true, true),
        "FarmerAgent" => (true, false),
        "HotelAgent" => (false, true),
        "HotelCustomer" => (false, true),
        "PublicPortalUser" => (false, true),
        // Unknown/unauthenticated caller: show neither price rather than guess.
        _ => (false, false)
    };
}
