using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FHSMS.TelegramBot.Clients.Models;
using FHSMS.TelegramBot.Configuration;
using Microsoft.Extensions.Options;

namespace FHSMS.TelegramBot.Clients;

/// <summary>Thrown when the FHSMS API rejects a request - carries the raw response body so the bot can show a real reason, not just "something went wrong".</summary>
public class FhsmsApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public FhsmsApiException(HttpStatusCode statusCode, string message) : base(message) => StatusCode = statusCode;
}

/// <summary>
/// Client for the FHSMS backend API. Every call this bot makes goes through
/// the exact same commands/endpoints the web PWA uses - the bot has no
/// business logic of its own, only conversation logic that ends in one of
/// these calls.
///
/// Every request carries an explicit bearer token per call rather than one
/// shared default header. This matters because the bot serves many chats -
/// many different logged-in agents plus anonymous hotel customers -
/// concurrently through a single long-lived instance of this class; a shared
/// DefaultRequestHeaders.Authorization would leak one user's identity into
/// another user's request. Two kinds of token are used:
///   - An agent's own token (bearerToken != null), obtained via LoginAsync
///     when they run /login - used for every hotel-agent/farmer-agent action,
///     so the API can correctly attribute orders, stock receipts, and the
///     commissions that follow to that specific agent.
///   - The configured service account's token (bearerToken == null, falls
///     back to _serviceAccountToken) - used only for the anonymous
///     hotel-customer self-ordering flow, where there is no agent to
///     attribute anything to.
/// </summary>
public class FhsmsApiClient
{
    private readonly HttpClient _http;
    private readonly FhsmsSettings _settings;
    private string? _serviceAccountToken;

    public FhsmsApiClient(HttpClient http, IOptions<FhsmsSettings> settings)
    {
        _settings = settings.Value;
        http.BaseAddress = new Uri(_settings.ApiBaseUrl.TrimEnd('/') + "/");
        _http = http;
    }

    /// <summary>Logs in the configured service account, used for anonymous/customer-facing calls. Called once at startup and again automatically if that token expires.</summary>
    public async Task LoginServiceAccountAsync(CancellationToken cancellationToken)
    {
        var response = await _http.PostAsJsonAsync(
            "auth/login",
            new LoginRequest { Email = _settings.ServiceAccountEmail, Password = _settings.ServiceAccountPassword },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Login to FHSMS API returned an empty response.");

        _serviceAccountToken = result.Token;
    }

    /// <summary>
    /// Logs a person in with their own FHSMS credentials (issued by an admin
    /// via the Users page - see AdminResetPasswordCommand). Returns null on
    /// bad credentials rather than throwing, so /login can reply with a plain
    /// "email or password is wrong" instead of a stack trace.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new LoginRequest { Email = email, Password = password }, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
    }

    public Task<List<ProductDto>> GetProductsAsync(string? token, CancellationToken cancellationToken)
        => SendAsync<List<ProductDto>>(HttpMethod.Get, "products?activeOnly=true", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<ProductDto>(), cancellationToken);

    /// <summary>Farmers a farmer agent can pick from when logging a stock receipt.</summary>
    public Task<List<FarmerDto>> GetFarmersAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<FarmerDto>>(HttpMethod.Get, "farmers", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<FarmerDto>(), cancellationToken);

    /// <summary>Hotels a hotel agent can pick from when logging an order.</summary>
    public Task<List<CustomerDto>> GetCustomersAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<CustomerDto>>(HttpMethod.Get, "customers", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<CustomerDto>(), cancellationToken);

    public Task<Guid> CreateFarmerAsync(string token, CreateFarmerRequest request, CancellationToken cancellationToken)
        => SendGuidAsync("farmers", request, token, cancellationToken);

    public Task<Guid> CreateCustomerAsync(string? token, CreateCustomerRequest request, CancellationToken cancellationToken)
        => SendGuidAsync("customers", request, token, cancellationToken);

    /// <summary>Logs stock received from a farmer - the flat-rate-per-quantity commission accrues automatically for whichever farmer agent's token this call used.</summary>
    public Task<Guid> RecordStockReceiptAsync(string token, RecordStockReceiptRequest request, CancellationToken cancellationToken)
        => SendGuidAsync("inventory/transactions", request, token, cancellationToken);

    /// <summary>Creates an order. Pass null token for the anonymous hotel-customer flow (uses the service account); pass an agent's token for the hotel-agent flow (commission accrues automatically).</summary>
    public Task<Guid> CreateOrderAsync(string? token, CreateOrderRequest request, CancellationToken cancellationToken)
        => SendGuidAsync("orders", request, token, cancellationToken);

    public Task<List<OrderSummaryDto>> GetOrdersByCustomerAsync(string? token, Guid customerId, CancellationToken cancellationToken)
        => SendAsync<List<OrderSummaryDto>>(HttpMethod.Get, $"orders?customerId={customerId}", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<OrderSummaryDto>(), cancellationToken);

    // --- Driver Portal (agent's own token required for all of these) ---

    public Task<DriverProfileDto?> GetMyDriverProfileAsync(string token, CancellationToken cancellationToken)
        => SendAsync<DriverProfileDto>(HttpMethod.Get, "drivers/profile/me", null, token, cancellationToken);

    public Task<Guid> RegisterDriverProfileAsync(string token, string fullName, string? phone, string? plateNumber, string truckType, CancellationToken cancellationToken)
        => SendGuidAsync("drivers/profile", new { fullName, phone, plateNumber, truckType }, token, cancellationToken);

    public Task<List<TripDto>> GetAvailableTripsAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<TripDto>>(HttpMethod.Get, "drivers/trips/available", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<TripDto>(), cancellationToken);

    public Task<List<TripDto>> GetMyTripsAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<TripDto>>(HttpMethod.Get, "drivers/trips/mine", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<TripDto>(), cancellationToken);

    public async Task AcceptTripAsync(string token, Guid deliveryId, CancellationToken cancellationToken)
        => await SendRawAsync(HttpMethod.Post, $"drivers/trips/{deliveryId}/accept", null, token, cancellationToken);

    /// <summary>Confirms handoff for a trip the driver accepted - the minimal fields the bot can practically collect (no signature/photo capture over chat; those remain a web-app-only extra, not required to close out a delivery).</summary>
    public async Task MarkTripDeliveredAsync(string token, Guid deliveryId, string? receivedByName, string? notes, CancellationToken cancellationToken)
        => await SendRawAsync(HttpMethod.Post, $"deliveries/{deliveryId}/delivered",
            new { notes, receivedByName, signatureImageBase64 = (string?)null, photoUrl = (string?)null, latitude = (double?)null, longitude = (double?)null },
            token, cancellationToken);

    // --- Commission (agent's own token required) ---

    public Task<List<CommissionDto>> GetMyCommissionsAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<CommissionDto>>(HttpMethod.Get, "commissions/mine", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<CommissionDto>(), cancellationToken);

    // --- Invoices (hotel agent's own token - GetInvoicesQueryHandler scopes to their own orders) ---

    public Task<List<InvoiceDto>> GetMyInvoicesAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<InvoiceDto>>(HttpMethod.Get, "invoices", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<InvoiceDto>(), cancellationToken);

    /// <summary>Starts a hosted-checkout payment (Chapa, Telebirr) - returns a URL to hand the payer; the payment itself is recorded later when the provider's webhook lands.</summary>
    public Task<InitiatePaymentResultDto?> InitiateInvoicePaymentAsync(string token, Guid invoiceId, string providerKey, CancellationToken cancellationToken)
        => SendAsync<InitiatePaymentResultDto>(HttpMethod.Post, $"invoices/{invoiceId}/pay/{providerKey}", null, token, cancellationToken);

    /// <summary>The "I already transferred the money" self-service path - verifies a reference against the provider's own public receipt lookup (CBE Birr today) and records the payment immediately if it checks out.</summary>
    public async Task VerifyInvoicePaymentAsync(
        string token, Guid invoiceId, string providerKey, string reference, string? secondaryIdentifier, Guid? bankAccountId, CancellationToken cancellationToken)
        => await SendRawAsync(HttpMethod.Post, $"invoices/{invoiceId}/verify-payment",
            new { providerKey, reference, secondaryIdentifier, bankAccountId }, token, cancellationToken);

    /// <summary>Which provider keys support automatic "I already paid" verification right now (see IReceiptVerifier on the server) - anything else falls back to the bank-transfer path.</summary>
    public Task<List<string>> GetPaymentVerifiersAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<string>>(HttpMethod.Get, "payments/verifiers", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<string>(), cancellationToken);

    /// <summary>Any bank in Ethiopia the company has registered - "which account do I transfer to" for the manual bank-payment path. Readable by any authenticated role, same as the web app.</summary>
    public Task<List<BankAccountDto>> GetBankAccountsAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<BankAccountDto>>(HttpMethod.Get, "bankaccounts", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<BankAccountDto>(), cancellationToken);

    /// <summary>Records a payment directly (the "any bank in Ethiopia" manual path - no automatic verifier exists for most banks, so this is recorded the same way a staff member's manual entry would be, for an admin to reconcile).</summary>
    public async Task RecordPaymentAsync(
        string token, Guid invoiceId, decimal amount, string method, string reference, Guid? bankAccountId, CancellationToken cancellationToken)
        => await SendRawAsync(HttpMethod.Post, "payments",
            new { invoiceId, amount, method, reference, bankAccountId }, token, cancellationToken);

    // --- Farmer invoices (farmer agent's own token - GetFarmerInvoicesQueryHandler scopes to their own stock-ins) ---

    public Task<List<FarmerInvoiceDto>> GetMyFarmerInvoicesAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<FarmerInvoiceDto>>(HttpMethod.Get, "farmerinvoices", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<FarmerInvoiceDto>(), cancellationToken);

    // --- Driver payments (driver's own token - GetDriverPaymentsQueryHandler scopes to their own trips) ---

    public Task<List<DriverPaymentDto>> GetMyDriverPaymentsAsync(string token, CancellationToken cancellationToken)
        => SendAsync<List<DriverPaymentDto>>(HttpMethod.Get, "driverpayments", null, token, cancellationToken)
            .ContinueWith(t => t.Result ?? new List<DriverPaymentDto>(), cancellationToken);

    public async Task<OrderSummaryDto?> GetOrderAsync(string? token, Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            return await SendAsync<OrderSummaryDto>(HttpMethod.Get, $"orders/{orderId}", null, token, cancellationToken);
        }
        catch (FhsmsApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>The truck/trip side of /orderstatus - null if no delivery has been posted for this order yet (order confirmed but not yet dispatched for pickup).</summary>
    public async Task<DeliveryDto?> GetDeliveryByOrderAsync(string? token, Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            return await SendAsync<DeliveryDto>(HttpMethod.Get, $"deliveries/by-order/{orderId}", null, token, cancellationToken);
        }
        catch (FhsmsApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private async Task<Guid> SendGuidAsync(string path, object body, string? token, CancellationToken cancellationToken)
    {
        var raw = await SendRawAsync(HttpMethod.Post, path, body, token, cancellationToken);
        return Guid.Parse(raw.Trim('"'));
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body, string? token, CancellationToken cancellationToken)
    {
        var raw = await SendRawAsync(method, path, body, token, cancellationToken);
        if (string.IsNullOrWhiteSpace(raw)) return default;
        return System.Text.Json.JsonSerializer.Deserialize<T>(raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private async Task<string> SendRawAsync(HttpMethod method, string path, object? body, string? token, CancellationToken cancellationToken)
    {
        async Task<HttpResponseMessage> SendOnceAsync(string bearerToken)
        {
            using var request = new HttpRequestMessage(method, path);
            if (body is not null) request.Content = JsonContent.Create(body);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            return await _http.SendAsync(request, cancellationToken);
        }

        var isServiceAccountCall = token is null;
        var effectiveToken = token ?? _serviceAccountToken
            ?? throw new InvalidOperationException("FHSMS service account is not logged in yet.");

        var response = await SendOnceAsync(effectiveToken);

        // Only the service account auto-reauthenticates on expiry - an
        // agent's own token expiring should surface as "please /login again",
        // not silently retry as someone else's identity.
        if (response.StatusCode == HttpStatusCode.Unauthorized && isServiceAccountCall)
        {
            await LoginServiceAccountAsync(cancellationToken);
            response = await SendOnceAsync(_serviceAccountToken!);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new FhsmsApiException(response.StatusCode, ExtractErrorMessage(responseBody));

        return responseBody;
    }

    private static string ExtractErrorMessage(string body)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("detail", out var detail))
                return detail.GetString() ?? body;
            if (doc.RootElement.TryGetProperty("title", out var title))
                return title.GetString() ?? body;
            if (doc.RootElement.TryGetProperty("errors", out var errors))
                return errors.ToString();
        }
        catch
        {
            // not JSON - fall through to raw body
        }
        return string.IsNullOrWhiteSpace(body) ? "Unknown error" : body;
    }
}
