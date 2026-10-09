using System.Text.Json.Serialization;

namespace FHSMS.TelegramBot.Clients.Models;

public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;

    [JsonPropertyName("password")]
    public string Password { get; set; } = default!;
}

public class LoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = default!;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = default!;

    [JsonPropertyName("role")]
    public string Role { get; set; } = default!;

    [JsonPropertyName("requiresTwoFactor")]
    public bool RequiresTwoFactor { get; set; }
}

public class ProductDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("sku")]
    public string Sku { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    // The API already applies role-based visibility (ProductPriceVisibility)
    // before the bot ever sees this response: a farmer agent's session gets
    // BuyingPrice populated and SellingPrice null, a hotel agent's session
    // gets the reverse. UnitPrice picks whichever one the caller's role is
    // allowed to see, so every flow below can keep using it unchanged.
    [JsonPropertyName("currentBuyingPrice")]
    public decimal? BuyingPrice { get; set; }

    [JsonPropertyName("currentSellingPrice")]
    public decimal? SellingPrice { get; set; }

    public decimal? UnitPrice => SellingPrice ?? BuyingPrice;

    [JsonPropertyName("unitAbbreviation")]
    public string? UnitAbbreviation { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

public class FarmerDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

public class CustomerDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

public class CreateFarmerRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("contactPerson")]
    public string? ContactPerson { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("bankAccountNumber")]
    public string? BankAccountNumber { get; set; }
}

public class CreateCustomerRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("contactPerson")]
    public string? ContactPerson { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("creditLimit")]
    public decimal? CreditLimit { get; set; }
}

public class RecordStockReceiptRequest
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Receiving";

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("orderId")]
    public Guid? OrderId { get; set; } = null;

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("farmerId")]
    public Guid? FarmerId { get; set; }

    // Deliberately NOT sent by this bot - the server derives the agent from
    // whichever farmer agent's token made the call (see
    // RecordInventoryTransactionCommandHandler), so a logged-in farmer agent
    // can never accidentally (or maliciously) log stock "as" someone else.
    [JsonPropertyName("agentUserId")]
    public Guid? AgentUserId { get; set; } = null;
}

public class CreateOrderItemRequest
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
}

public class CreateOrderRequest
{
    [JsonPropertyName("customerId")]
    public Guid CustomerId { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = "Telegram";

    // Deliberately NOT sent by this bot for agent-placed orders - the server
    // derives the agent from whichever hotel agent's token made the call
    // (see CreateOrderCommandHandler). Left settable only so the anonymous
    // hotel-customer self-order path (which uses the service account, not an
    // agent token) still works exactly as before.
    [JsonPropertyName("agentUserId")]
    public Guid? AgentUserId { get; set; }

    [JsonPropertyName("items")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

/// <summary>Trimmed-down mirror of OrderDto - only the fields the bot's /myorders and /orderstatus commands display.</summary>
public class OrderSummaryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; } = default!;

    [JsonPropertyName("customerId")]
    public Guid CustomerId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }
}

public enum BotTruckType { Pickup, SmallTruck, MediumTruck, Isuzu, HeavyTruck, Trailer, Other }

public class DriverProfileDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = default!;

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("plateNumber")]
    public string? PlateNumber { get; set; }

    [JsonPropertyName("truckType")]
    public string TruckType { get; set; } = default!;
}

public class TripDto
{
    [JsonPropertyName("deliveryId")]
    public Guid DeliveryId { get; set; }

    [JsonPropertyName("orderId")]
    public Guid OrderId { get; set; }

    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; } = default!;

    [JsonPropertyName("originLocation")]
    public string? OriginLocation { get; set; }

    [JsonPropertyName("destinationAddress")]
    public string DestinationAddress { get; set; } = default!;

    [JsonPropertyName("destinationName")]
    public string DestinationName { get; set; } = default!;

    [JsonPropertyName("productSummary")]
    public string ProductSummary { get; set; } = default!;

    [JsonPropertyName("tripPrice")]
    public decimal? TripPrice { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
}

public class CommissionDto
{
    [JsonPropertyName("commissionAmount")]
    public decimal CommissionAmount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>The truck/trip side of an order - separate from OrderSummaryDto.Status (Draft/Confirmed/Preparing/Shipped/...), this is where the delivery actually is right now. Mirrors the web app's DeliveryDto, trimmed to what /orderstatus shows in chat.</summary>
public class DeliveryDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("driverName")]
    public string? DriverName { get; set; }

    [JsonPropertyName("vehicleInfo")]
    public string? VehicleInfo { get; set; }

    [JsonPropertyName("originLocation")]
    public string? OriginLocation { get; set; }

    [JsonPropertyName("destinationAddress")]
    public string DestinationAddress { get; set; } = default!;

    [JsonPropertyName("dispatchedAt")]
    public DateTime? DispatchedAt { get; set; }

    [JsonPropertyName("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }

    [JsonPropertyName("receivedByName")]
    public string? ReceivedByName { get; set; }
}

/// <summary>The hotel-side (selling) invoice - trimmed to what the bot's /myinvoices and /pay flows need. Mirrors the web app's InvoiceDto.</summary>
public class InvoiceDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }

    [JsonPropertyName("taxAmount")]
    public decimal TaxAmount { get; set; }

    [JsonPropertyName("platformCommissionAmount")]
    public decimal PlatformCommissionAmount { get; set; }

    [JsonPropertyName("hotelAgentBonusAmount")]
    public decimal HotelAgentBonusAmount { get; set; }

    [JsonPropertyName("grandTotal")]
    public decimal GrandTotal { get; set; }

    [JsonPropertyName("amountPaid")]
    public decimal AmountPaid { get; set; }

    [JsonPropertyName("balanceDue")]
    public decimal BalanceDue { get; set; }
}

/// <summary>Result of POST /invoices/{id}/pay/{providerKey} - a hosted-checkout URL to hand the payer (Chapa, Telebirr).</summary>
public class InitiatePaymentResultDto
{
    [JsonPropertyName("checkoutUrl")]
    public string? CheckoutUrl { get; set; }

    [JsonPropertyName("isSuccessful")]
    public bool IsSuccessful { get; set; }

    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; set; }
}

/// <summary>The buying-side (farmer) invoice - trimmed to what /myfarmerinvoices shows. Mirrors the web app's FarmerInvoiceDto.</summary>
public class FarmerInvoiceDto
{
    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; set; } = default!;

    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("unitAbbreviation")]
    public string? UnitAbbreviation { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>The driver-side counterpart to FarmerInvoiceDto - trimmed to what /mypayments shows. Mirrors the web app's DriverPaymentDto.</summary>
public class DriverPaymentDto
{
    [JsonPropertyName("destinationAddress")]
    public string? DestinationAddress { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("driverWasPaid")]
    public bool DriverWasPaid { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>Where to transfer for a manual bank payment - mirrors the web app's BankAccountDto.</summary>
public class BankAccountDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("bankName")]
    public string BankName { get; set; } = default!;

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = default!;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = default!;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

