using FHSMS.TelegramBot.Localization;

namespace FHSMS.TelegramBot.Conversation;

public enum ConversationStep
{
    Idle,

    // Agent login (/login)
    AwaitingLoginEmail,
    AwaitingLoginPassword,

    // Hotel-customer self-order flow (unchanged from before - runs under the service account, no agent login needed)
    AwaitingProductSelection,
    AwaitingQuantity,
    AwaitingConfirmation,

    // Hotel agent: register a new hotel (/newhotel)
    AwaitingNewHotelName,
    AwaitingNewHotelPhone,
    AwaitingNewHotelAddress,

    // Hotel agent: log an order for a hotel (/neworder)
    AwaitingOrderHotelSelection,
    AwaitingOrderProductSelection,
    AwaitingOrderQuantity,
    AwaitingOrderConfirmation,

    // Farmer agent: register a new farmer (/newfarmer)
    AwaitingNewFarmerName,
    AwaitingNewFarmerPhone,
    AwaitingNewFarmerLocation,

    // Farmer agent: log stock received (/stockin)
    AwaitingStockFarmerSelection,
    AwaitingStockProductSelection,
    AwaitingStockQuantity,
    AwaitingStockConfirmation,

    // Driver: register/update profile (/registerdriver)
    AwaitingDriverName,
    AwaitingDriverPhone,
    AwaitingDriverPlate,
    AwaitingDriverTruckType,

    // Driver: accept a trip from the board (/trips)
    AwaitingTripSelection,

    // Driver: confirm handoff on an accepted trip (/deliver)
    AwaitingDeliverTripSelection,
    AwaitingDeliverReceivedByName,

    // Any logged-in agent: pay an invoice (/pay)
    AwaitingPayInvoiceSelection,
    AwaitingPayMethodSelection,
    AwaitingPayCbeBirrReference,
    AwaitingPayCbeBirrPhone,
    AwaitingPayBankSelection,
    AwaitingPayBankReference
}

public record CartItem(Guid ProductId, string ProductName, decimal UnitPrice, decimal Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

/// <summary>
/// Per-chat conversation state. Kept in memory (see InMemoryBotStateStore) -
/// fine for a single bot instance / demo purposes; swap in a database-backed
/// IBotStateStore before running this in a way that must survive restarts or
/// scale to multiple instances.
///
/// Two identities can occupy a chat, and they're independent:
///   - AgentToken/AgentRole/... - set by /login, for a hotel or farmer agent
///     acting as themselves (their commissions accrue to them).
///   - CustomerId - set by /register, for a hotel customer ordering for
///     themselves directly, with no agent involved at all.
/// A chat is normally one or the other, not both, but nothing enforces that -
/// an agent could also register their own hotel's customer account in the
/// same chat if that's ever useful.
/// </summary>
public class BotSession
{
    public long ChatId { get; init; }
    public BotLanguage Language { get; set; } = BotLanguage.English;
    public ConversationStep Step { get; set; } = ConversationStep.Idle;

    // --- Hotel customer self-order identity ---
    public Guid? CustomerId { get; set; }

    // --- Agent identity (set by /login) ---
    public string? AgentToken { get; set; }
    public string? AgentRole { get; set; } // "HotelAgent" or "FarmerAgent"
    public string? AgentFullName { get; set; }
    public string? PendingLoginEmail { get; set; }

    public bool IsAgentLoggedIn => AgentToken is not null;

    // --- Shared "pick a product, pick a quantity, repeat" cart used by both the customer order flow and the hotel-agent order flow ---
    public List<Clients.Models.ProductDto> LastShownProducts { get; set; } = new();
    public Clients.Models.ProductDto? PendingProduct { get; set; }
    public List<CartItem> Cart { get; } = new();

    // --- Hotel-agent: register-hotel flow scratch space ---
    public string? NewHotelName { get; set; }
    public string? NewHotelPhone { get; set; }

    // --- Hotel-agent: log-order flow scratch space ---
    public List<Clients.Models.CustomerDto> LastShownHotels { get; set; } = new();
    public Clients.Models.CustomerDto? SelectedHotel { get; set; }

    // --- Farmer-agent: register-farmer flow scratch space ---
    public string? NewFarmerName { get; set; }
    public string? NewFarmerPhone { get; set; }

    // --- Farmer-agent: log-stock flow scratch space ---
    public List<Clients.Models.FarmerDto> LastShownFarmers { get; set; } = new();
    public Clients.Models.FarmerDto? SelectedFarmer { get; set; }
    public Clients.Models.ProductDto? StockProduct { get; set; }

    // --- Driver: register/update profile flow scratch space ---
    public string? NewDriverName { get; set; }
    public string? NewDriverPhone { get; set; }
    public string? NewDriverPlate { get; set; }

    // --- Driver: trip board scratch space ---
    public List<Clients.Models.TripDto> LastShownTrips { get; set; } = new();
    public Clients.Models.TripDto? SelectedTripForDelivery { get; set; }

    // --- Pay-an-invoice flow scratch space (/pay) ---
    public List<Clients.Models.InvoiceDto> LastShownInvoices { get; set; } = new();
    public Clients.Models.InvoiceDto? SelectedInvoiceForPayment { get; set; }
    public string? SelectedPaymentProviderKey { get; set; } // "chapa", "telebirr", "cbebirr", or "bank"
    public string? PendingPaymentReference { get; set; }
    public List<Clients.Models.BankAccountDto> LastShownBankAccounts { get; set; } = new();
    public Clients.Models.BankAccountDto? SelectedBankAccountForPayment { get; set; }

    public void ResetOrder()
    {
        Step = ConversationStep.Idle;
        LastShownProducts.Clear();
        PendingProduct = null;
        Cart.Clear();
        NewHotelName = null;
        NewHotelPhone = null;
        LastShownHotels.Clear();
        SelectedHotel = null;
        NewFarmerName = null;
        NewFarmerPhone = null;
        LastShownFarmers.Clear();
        SelectedFarmer = null;
        StockProduct = null;
        NewDriverName = null;
        NewDriverPhone = null;
        NewDriverPlate = null;
        LastShownTrips.Clear();
        SelectedTripForDelivery = null;
        LastShownInvoices.Clear();
        SelectedInvoiceForPayment = null;
        SelectedPaymentProviderKey = null;
        PendingPaymentReference = null;
        LastShownBankAccounts.Clear();
        SelectedBankAccountForPayment = null;
    }

    public void LogoutAgent()
    {
        AgentToken = null;
        AgentRole = null;
        AgentFullName = null;
        PendingLoginEmail = null;
        ResetOrder();
    }
}
