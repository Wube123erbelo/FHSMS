using System.Globalization;
using System.Text;
using FHSMS.TelegramBot.Clients;
using FHSMS.TelegramBot.Clients.Models;
using FHSMS.TelegramBot.Localization;

namespace FHSMS.TelegramBot.Conversation;

/// <summary>
/// The bot's entire conversation logic. Every path that ends in placing an
/// order, registering a hotel/farmer, or logging stock calls exactly the same
/// FHSMS API commands the web PWA uses (CreateOrderCommand,
/// CreateCustomerCommand, CreateFarmerCommand, RecordInventoryTransactionCommand)
/// - this class only ever turns chat messages into one of those calls, it
/// never encodes order, tax, or commission rules itself.
///
/// Two audiences are served, matching the client's actual field team:
///   - Hotel and farmer agents (students), who /login with credentials an
///     admin issued them, then register hotels/farmers and log
///     orders/stock as themselves - see HandleAgentCommandAsync and the
///     Awaiting* steps below "AwaitingLoginPassword".
///   - Hotel customers ordering for themselves with no agent involved,
///     via /register (self-service, no admin-issued ID needed) then /order -
///     this is the original flow, kept working exactly as before.
/// </summary>
public class ConversationHandler
{
    private readonly TelegramApiClient _telegram;
    private readonly FhsmsApiClient _fhsms;
    private readonly IBotStateStore _stateStore;

    public ConversationHandler(TelegramApiClient telegram, FhsmsApiClient fhsms, IBotStateStore stateStore)
    {
        _telegram = telegram;
        _fhsms = fhsms;
        _stateStore = stateStore;
    }

    public async Task HandleAsync(TelegramMessage message, CancellationToken cancellationToken)
    {
        var session = _stateStore.GetOrCreate(message.Chat.Id);
        var text = message.Text?.Trim() ?? string.Empty;

        if (text.StartsWith('/'))
        {
            await HandleCommandAsync(session, text, cancellationToken);
            return;
        }

        switch (session.Step)
        {
            case ConversationStep.AwaitingLoginEmail:
                await HandleLoginEmailAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingLoginPassword:
                await HandleLoginPasswordAsync(session, text, cancellationToken);
                break;

            case ConversationStep.AwaitingProductSelection:
                await HandleProductSelectionAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingQuantity:
                await HandleQuantityAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingConfirmation:
                await Reply(session, "orderConfirmedFallback", cancellationToken);
                break;

            case ConversationStep.AwaitingNewHotelName:
                await HandleNewHotelNameAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingNewHotelPhone:
                await HandleNewHotelPhoneAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingNewHotelAddress:
                await HandleNewHotelAddressAsync(session, text, cancellationToken);
                break;

            case ConversationStep.AwaitingOrderHotelSelection:
                await HandleOrderHotelSelectionAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingOrderProductSelection:
                await HandleOrderProductSelectionAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingOrderQuantity:
                await HandleOrderQuantityAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingOrderConfirmation:
                await Reply(session, "orderConfirmedFallback", cancellationToken);
                break;

            case ConversationStep.AwaitingNewFarmerName:
                await HandleNewFarmerNameAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingNewFarmerPhone:
                await HandleNewFarmerPhoneAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingNewFarmerLocation:
                await HandleNewFarmerLocationAsync(session, text, cancellationToken);
                break;

            case ConversationStep.AwaitingStockFarmerSelection:
                await HandleStockFarmerSelectionAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingStockProductSelection:
                await HandleStockProductSelectionAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingStockQuantity:
                await HandleStockQuantityAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingStockConfirmation:
                await Reply(session, "stockConfirmedFallback", cancellationToken);
                break;

            case ConversationStep.AwaitingDriverName:
                await HandleDriverNameAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingDriverPhone:
                await HandleDriverPhoneAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingDriverPlate:
                await HandleDriverPlateAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingDriverTruckType:
                await HandleDriverTruckTypeAsync(session, text, cancellationToken);
                break;

            case ConversationStep.AwaitingTripSelection:
                await HandleTripSelectionAsync(session, text, cancellationToken);
                break;

            case ConversationStep.AwaitingDeliverTripSelection:
                await HandleDeliverTripSelectionAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingDeliverReceivedByName:
                await HandleDeliverReceivedByNameAsync(session, text, cancellationToken);
                break;

            case ConversationStep.AwaitingPayInvoiceSelection:
                await HandlePayInvoiceSelectionAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingPayMethodSelection:
                await HandlePayMethodSelectionAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingPayCbeBirrReference:
                await HandlePayCbeBirrReferenceAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingPayCbeBirrPhone:
                await HandlePayCbeBirrPhoneAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingPayBankSelection:
                await HandlePayBankSelectionAsync(session, text, cancellationToken);
                break;
            case ConversationStep.AwaitingPayBankReference:
                await HandlePayBankReferenceAsync(session, text, cancellationToken);
                break;

            default:
                await Reply(session, "unknownCommand", cancellationToken);
                break;
        }
    }

    private async Task HandleCommandAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].Split('@')[0].ToLowerInvariant(); // strip "@BotName" suffix Telegram groups sometimes add
        var arg = parts.Length > 1 ? parts[1].Trim() : null;

        switch (command)
        {
            case "/start":
                await Reply(session, "welcome", cancellationToken);
                break;

            case "/help":
                await SendHelpAsync(session, cancellationToken);
                break;

            case "/language":
                await HandleLanguageAsync(session, arg, cancellationToken);
                break;

            // --- Agent login/logout - available to everyone ---
            case "/login":
                await StartLoginAsync(session, cancellationToken);
                break;
            case "/logout":
                session.LogoutAgent();
                await Reply(session, "loggedOut", cancellationToken);
                break;
            case "/whoami":
                await HandleWhoAmIAsync(session, cancellationToken);
                break;

            // --- Hotel customer self-order flow (no agent login needed) ---
            case "/register":
                await StartCustomerRegistrationAsync(session, cancellationToken);
                break;
            case "/order":
                await StartOrderAsync(session, cancellationToken);
                break;
            case "/myorders":
                await HandleMyOrdersAsync(session, cancellationToken);
                break;
            case "/orderstatus":
                await HandleOrderStatusAsync(session, arg, cancellationToken);
                break;
            case "/done":
                if (session.Step == ConversationStep.AwaitingOrderProductSelection)
                    await HandleAgentOrderDoneAsync(session, cancellationToken);
                else
                    await HandleDoneAsync(session, cancellationToken);
                break;
            case "/confirm":
                if (session.Step == ConversationStep.AwaitingOrderConfirmation)
                    await HandleAgentOrderConfirmAsync(session, cancellationToken);
                else
                    await HandleConfirmAsync(session, cancellationToken);
                break;

            // --- Hotel agent flow ---
            case "/newhotel":
                await RequireRoleThenAsync(session, "HotelAgent", cancellationToken, () => StartNewHotelAsync(session, cancellationToken));
                break;
            case "/neworder":
                await RequireRoleThenAsync(session, "HotelAgent", cancellationToken, () => StartAgentOrderAsync(session, cancellationToken));
                break;

            // --- Farmer agent flow ---
            case "/newfarmer":
                await RequireRoleThenAsync(session, "FarmerAgent", cancellationToken, () => StartNewFarmerAsync(session, cancellationToken));
                break;
            case "/stockin":
                await RequireRoleThenAsync(session, "FarmerAgent", cancellationToken, () => StartStockInAsync(session, cancellationToken));
                break;
            case "/stockdone":
                await HandleStockDoneAsync(session, cancellationToken);
                break;
            case "/stockconfirm":
                await HandleStockConfirmAsync(session, cancellationToken);
                break;

            // --- Commission - hotel and farmer agents only ---
            case "/mycommission":
                await RequireLoggedInThenAsync(session, cancellationToken, () => HandleMyCommissionAsync(session, cancellationToken));
                break;

            // --- Invoices & payment (any logged-in agent - server-side scoping decides what they actually see) ---
            case "/myinvoices":
                await RequireLoggedInThenAsync(session, cancellationToken, () => HandleMyInvoicesAsync(session, cancellationToken));
                break;
            case "/pay":
                await RequireLoggedInThenAsync(session, cancellationToken, () => StartPayAsync(session, cancellationToken));
                break;
            case "/myfarmerinvoices":
                await RequireRoleThenAsync(session, "FarmerAgent", cancellationToken, () => HandleMyFarmerInvoicesAsync(session, cancellationToken));
                break;
            case "/mypayments":
                await RequireRoleThenAsync(session, "Driver", cancellationToken, () => HandleMyDriverPaymentsAsync(session, cancellationToken));
                break;

            // --- Driver flow ---
            case "/registerdriver":
                await RequireRoleThenAsync(session, "Driver", cancellationToken, () => StartRegisterDriverAsync(session, cancellationToken));
                break;
            case "/trips":
                await RequireRoleThenAsync(session, "Driver", cancellationToken, () => StartTripsAsync(session, cancellationToken));
                break;
            case "/mytrips":
                await RequireRoleThenAsync(session, "Driver", cancellationToken, () => HandleMyTripsAsync(session, cancellationToken));
                break;
            case "/deliver":
                await RequireRoleThenAsync(session, "Driver", cancellationToken, () => StartDeliverAsync(session, cancellationToken));
                break;

            case "/cancel":
                session.ResetOrder();
                await Reply(session, "orderCancelled", cancellationToken);
                break;

            default:
                await Reply(session, "unknownCommand", cancellationToken);
                break;
        }
    }

    private async Task SendHelpAsync(BotSession session, CancellationToken cancellationToken)
    {
        if (session.AgentRole == "HotelAgent")
        {
            await Reply(session, "helpHotelAgent", cancellationToken, session.AgentFullName ?? "");
        }
        else if (session.AgentRole == "FarmerAgent")
        {
            await Reply(session, "helpFarmerAgent", cancellationToken, session.AgentFullName ?? "");
        }
        else if (session.AgentRole == "Driver")
        {
            await Reply(session, "helpDriver", cancellationToken, session.AgentFullName ?? "");
        }
        else
        {
            await Reply(session, "help", cancellationToken);
        }
    }

    private async Task HandleWhoAmIAsync(BotSession session, CancellationToken cancellationToken)
    {
        if (session.IsAgentLoggedIn)
            await Reply(session, "whoAmIAgent", cancellationToken, session.AgentFullName ?? "", session.AgentRole ?? "");
        else if (session.CustomerId is not null)
            await Reply(session, "whoAmICustomer", cancellationToken, session.CustomerId.Value);
        else
            await Reply(session, "whoAmINone", cancellationToken);
    }

    private async Task HandleLanguageAsync(BotSession session, string? arg, CancellationToken cancellationToken)
    {
        switch (arg?.ToLowerInvariant())
        {
            case "en":
                session.Language = BotLanguage.English;
                await Reply(session, "languageSet", cancellationToken);
                break;
            case "am":
                session.Language = BotLanguage.Amharic;
                await Reply(session, "languageSet", cancellationToken);
                break;
            default:
                await Reply(session, "languageUsage", cancellationToken);
                break;
        }
    }

    // ---------------------------------------------------------------
    // Agent login (/login) - two-step: email, then password. On success
    // the chat is now "signed in" as that agent for every subsequent
    // /newhotel, /neworder, /newfarmer, /stockin call until /logout.
    // ---------------------------------------------------------------

    private async Task StartLoginAsync(BotSession session, CancellationToken cancellationToken)
    {
        session.ResetOrder();
        session.Step = ConversationStep.AwaitingLoginEmail;
        await Reply(session, "loginAskEmail", cancellationToken);
    }

    private async Task HandleLoginEmailAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.PendingLoginEmail = text;
        session.Step = ConversationStep.AwaitingLoginPassword;
        await Reply(session, "loginAskPassword", cancellationToken);
    }

    private async Task HandleLoginPasswordAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        var email = session.PendingLoginEmail ?? "";
        session.PendingLoginEmail = null;
        session.Step = ConversationStep.Idle;

        var result = await _fhsms.LoginAsync(email, text, cancellationToken);

        if (result is null)
        {
            await Reply(session, "loginFailed", cancellationToken);
            return;
        }

        if (result.RequiresTwoFactor)
        {
            await Reply(session, "loginRequiresTwoFactor", cancellationToken);
            return;
        }

        if (result.Role is not ("HotelAgent" or "FarmerAgent" or "Driver" or "SuperAdmin"))
        {
            await Reply(session, "loginWrongRole", cancellationToken, result.Role);
            return;
        }

        session.AgentToken = result.Token;
        session.AgentRole = result.Role == "SuperAdmin" ? "HotelAgent" : result.Role; // let an admin test the hotel-agent flow by default
        session.AgentFullName = result.FullName;

        if (result.Role == "HotelAgent" || result.Role == "SuperAdmin")
            await Reply(session, "loginSuccessHotelAgent", cancellationToken, result.FullName);
        else if (result.Role == "FarmerAgent")
            await Reply(session, "loginSuccessFarmerAgent", cancellationToken, result.FullName);
        else
            await Reply(session, "loginSuccessDriver", cancellationToken, result.FullName);
    }

    private async Task RequireRoleThenAsync(BotSession session, string role, CancellationToken cancellationToken, Func<Task> action)
    {
        if (!session.IsAgentLoggedIn)
        {
            await Reply(session, "notLoggedIn", cancellationToken);
            return;
        }
        if (session.AgentRole != role)
        {
            await Reply(session, "wrongAgentRole", cancellationToken, role);
            return;
        }
        await action();
    }

    /// <summary>For commands any logged-in agent can use regardless of which specific role they are - e.g. /mycommission (hotel and farmer agents both earn commission; drivers don't, but StartRegisterDriverAsync etc. already gate on the Driver role specifically).</summary>
    private async Task RequireLoggedInThenAsync(BotSession session, CancellationToken cancellationToken, Func<Task> action)
    {
        if (!session.IsAgentLoggedIn)
        {
            await Reply(session, "notLoggedIn", cancellationToken);
            return;
        }
        await action();
    }

    // ---------------------------------------------------------------
    // Commission - hotel and farmer agents (/mycommission). Drivers earn
    // trip prices, not this kind of commission, so this simply won't have
    // much to show for a Driver account, but there's no harm in allowing it.
    // ---------------------------------------------------------------

    private async Task HandleMyCommissionAsync(BotSession session, CancellationToken cancellationToken)
    {
        var commissions = await _fhsms.GetMyCommissionsAsync(session.AgentToken!, cancellationToken);
        if (commissions.Count == 0)
        {
            await Reply(session, "noCommissionYet", cancellationToken);
            return;
        }

        var today = DateTime.UtcNow.Date;
        var todaysTotal = commissions.Where(c => c.CreatedAt.Date == today).Sum(c => c.CommissionAmount);
        var monthTotal = commissions
            .Where(c => c.CreatedAt.Year == DateTime.UtcNow.Year && c.CreatedAt.Month == DateTime.UtcNow.Month)
            .Sum(c => c.CommissionAmount);
        var allTimeTotal = commissions.Sum(c => c.CommissionAmount);

        await Reply(session, "myCommission", cancellationToken,
            todaysTotal.ToString("0.##"), monthTotal.ToString("0.##"), allTimeTotal.ToString("0.##"), commissions.Count);
    }

    // ---------------------------------------------------------------
    // Invoices & payment (/myinvoices, /pay). Every figure here comes
    // straight from the same Invoice fields the web app shows - subtotal,
    // tax, platform commission, hotel agent bonus, grand total - so a hotel
    // agent checking from their phone sees exactly the same breakdown as
    // the invoice detail page.
    // ---------------------------------------------------------------

    private async Task HandleMyInvoicesAsync(BotSession session, CancellationToken cancellationToken)
    {
        var invoices = await _fhsms.GetMyInvoicesAsync(session.AgentToken!, cancellationToken);
        if (invoices.Count == 0)
        {
            await Reply(session, "noInvoicesYet", cancellationToken);
            return;
        }

        var sb = new StringBuilder();
        foreach (var inv in invoices.OrderByDescending(i => i.BalanceDue > 0).ThenBy(i => i.InvoiceNumber))
        {
            sb.AppendLine($"{inv.InvoiceNumber} - {inv.Status} - {inv.GrandTotal:0.##} ETB " +
                $"({(inv.BalanceDue > 0 ? $"{inv.BalanceDue:0.##} ETB due" : "paid")})");
        }

        await Reply(session, "myInvoicesHeader", cancellationToken, sb.ToString());
    }

    private async Task StartPayAsync(BotSession session, CancellationToken cancellationToken)
    {
        var invoices = await _fhsms.GetMyInvoicesAsync(session.AgentToken!, cancellationToken);
        var unpaid = invoices.Where(i => i.BalanceDue > 0 && i.Status != "Cancelled").ToList();
        session.ResetOrder();

        if (unpaid.Count == 0)
        {
            await Reply(session, "noUnpaidInvoices", cancellationToken);
            return;
        }

        session.LastShownInvoices = unpaid;
        session.Step = ConversationStep.AwaitingPayInvoiceSelection;

        var sb = new StringBuilder();
        for (var i = 0; i < unpaid.Count; i++)
            sb.AppendLine($"{i + 1}. {unpaid[i].InvoiceNumber} - {unpaid[i].BalanceDue:0.##} ETB due");
        await Reply(session, "choosePayInvoice", cancellationToken, sb.ToString());
    }

    private async Task HandlePayInvoiceSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, out var number) || number < 1 || number > session.LastShownInvoices.Count)
        {
            await Reply(session, "invalidInvoiceNumber", cancellationToken);
            return;
        }

        session.SelectedInvoiceForPayment = session.LastShownInvoices[number - 1];

        // Only offer automatic hosted-checkout providers and the ones
        // IReceiptVerifier actually supports right now - everything else
        // (most individual banks) falls back to the manual "transfer, then
        // tell us the reference" path via option 4.
        var verifiers = await _fhsms.GetPaymentVerifiersAsync(session.AgentToken!, cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("1. Chapa");
        sb.AppendLine("2. Telebirr");
        if (verifiers.Contains("cbebirr", StringComparer.OrdinalIgnoreCase))
            sb.AppendLine("3. CBE Birr");
        sb.AppendLine("4. " + BotTranslations.Get(session.Language, "anyBankOption"));

        session.Step = ConversationStep.AwaitingPayMethodSelection;
        await Reply(session, "choosePayMethod", cancellationToken, sb.ToString());
    }

    private async Task HandlePayMethodSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        var invoice = session.SelectedInvoiceForPayment;
        if (invoice is null) { session.Step = ConversationStep.Idle; return; }

        switch (text.Trim())
        {
            case "1":
            case "2":
                var providerKey = text.Trim() == "1" ? "chapa" : "telebirr";
                session.Step = ConversationStep.Idle;
                try
                {
                    var result = await _fhsms.InitiateInvoicePaymentAsync(session.AgentToken!, invoice.Id, providerKey, cancellationToken);
                    if (result?.CheckoutUrl is { Length: > 0 } url)
                        await Reply(session, "payOnlineLink", cancellationToken, url);
                    else
                        await Reply(session, "actionFailed", cancellationToken, result?.FailureReason ?? "Unknown error");
                }
                catch (FhsmsApiException ex)
                {
                    await Reply(session, "actionFailed", cancellationToken, ex.Message);
                }
                break;

            case "3":
                session.Step = ConversationStep.AwaitingPayCbeBirrReference;
                await Reply(session, "askCbeBirrReference", cancellationToken);
                break;

            case "4":
                var accounts = (await _fhsms.GetBankAccountsAsync(session.AgentToken!, cancellationToken))
                    .Where(a => a.IsActive).ToList();
                if (accounts.Count == 0)
                {
                    session.Step = ConversationStep.Idle;
                    await Reply(session, "noBankAccounts", cancellationToken);
                    break;
                }
                session.LastShownBankAccounts = accounts;
                session.Step = ConversationStep.AwaitingPayBankSelection;
                var sb = new StringBuilder();
                for (var i = 0; i < accounts.Count; i++)
                    sb.AppendLine($"{i + 1}. {accounts[i].BankName} - {accounts[i].AccountName} ({accounts[i].AccountNumber})");
                await Reply(session, "chooseBankAccount", cancellationToken, sb.ToString());
                break;

            default:
                await Reply(session, "invalidChoice", cancellationToken);
                break;
        }
    }

    private async Task HandlePayCbeBirrReferenceAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.PendingPaymentReference = text.Trim();
        session.Step = ConversationStep.AwaitingPayCbeBirrPhone;
        await Reply(session, "askCbeBirrPhone", cancellationToken);
    }

    private async Task HandlePayCbeBirrPhoneAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        var invoice = session.SelectedInvoiceForPayment;
        var reference = session.PendingPaymentReference;
        session.Step = ConversationStep.Idle;
        if (invoice is null || reference is null) return;

        try
        {
            await _fhsms.VerifyInvoicePaymentAsync(session.AgentToken!, invoice.Id, "cbebirr", reference, text.Trim(), null, cancellationToken);
            await Reply(session, "paymentVerified", cancellationToken, invoice.InvoiceNumber);
        }
        catch (FhsmsApiException ex)
        {
            // A verification failure (wrong reference, amount mismatch, the
            // endpoint being unreachable) is a legitimate outcome, not a bug -
            // point them at the manual fallback rather than a dead end.
            await Reply(session, "paymentVerificationFailed", cancellationToken, ex.Message);
        }
    }

    private async Task HandlePayBankSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, out var number) || number < 1 || number > session.LastShownBankAccounts.Count)
        {
            await Reply(session, "invalidChoice", cancellationToken);
            return;
        }

        session.SelectedBankAccountForPayment = session.LastShownBankAccounts[number - 1];
        session.Step = ConversationStep.AwaitingPayBankReference;
        var a = session.SelectedBankAccountForPayment;
        await Reply(session, "askBankReference", cancellationToken, $"{a.BankName} - {a.AccountName} ({a.AccountNumber})");
    }

    private async Task HandlePayBankReferenceAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        var invoice = session.SelectedInvoiceForPayment;
        var account = session.SelectedBankAccountForPayment;
        session.Step = ConversationStep.Idle;
        if (invoice is null) return;

        try
        {
            // No automatic verifier exists for most banks - this is recorded
            // the same way a staff member's manual entry would be
            // (RecordPaymentCommand), for an admin to reconcile against the
            // bank statement, same principle as the web app's "any bank"
            // fallback path.
            await _fhsms.RecordPaymentAsync(session.AgentToken!, invoice.Id, invoice.BalanceDue, "Bank", text.Trim(), account?.Id, cancellationToken);
            await Reply(session, "paymentRecordedPendingReconciliation", cancellationToken, invoice.InvoiceNumber);
        }
        catch (FhsmsApiException ex)
        {
            await Reply(session, "actionFailed", cancellationToken, ex.Message);
        }
    }

    // ---------------------------------------------------------------
    // Farmer agent: view farmer invoices (/myfarmerinvoices) - what the
    // company owes for produce this agent has logged, auto-generated at
    // stock-in time, never hand-typed.
    // ---------------------------------------------------------------

    private async Task HandleMyFarmerInvoicesAsync(BotSession session, CancellationToken cancellationToken)
    {
        var invoices = await _fhsms.GetMyFarmerInvoicesAsync(session.AgentToken!, cancellationToken);
        if (invoices.Count == 0)
        {
            await Reply(session, "noFarmerInvoicesYet", cancellationToken);
            return;
        }

        var sb = new StringBuilder();
        foreach (var inv in invoices)
        {
            sb.AppendLine($"{inv.InvoiceNumber} - {inv.ProductName} - {inv.Quantity} {inv.UnitAbbreviation} - " +
                $"{inv.TotalAmount:0.##} ETB - {inv.Status}");
        }

        await Reply(session, "myFarmerInvoicesHeader", cancellationToken, sb.ToString());
    }

    // ---------------------------------------------------------------
    // Driver: view trip payments (/mypayments) - what the company owes for
    // trips this driver has delivered, auto-generated at drop-off, separate
    // from any commission (there is no commission on the driver side).
    // ---------------------------------------------------------------

    private async Task HandleMyDriverPaymentsAsync(BotSession session, CancellationToken cancellationToken)
    {
        var payments = await _fhsms.GetMyDriverPaymentsAsync(session.AgentToken!, cancellationToken);
        if (payments.Count == 0)
        {
            await Reply(session, "noDriverPaymentsYet", cancellationToken);
            return;
        }

        var sb = new StringBuilder();
        foreach (var p in payments)
        {
            sb.AppendLine($"{p.DestinationAddress ?? "-"} - {p.Amount:0.##} ETB - {p.Status} - " +
                (p.DriverWasPaid ? "paid" : "not yet paid"));
        }

        await Reply(session, "myDriverPaymentsHeader", cancellationToken, sb.ToString());
    }

    // ---------------------------------------------------------------
    // Driver: register/update profile (/registerdriver). Exactly one
    // profile per logged-in driver account - RegisterDriverProfileCommand
    // on the server creates it on first use and updates it every time
    // after, so there is no way to end up with two vehicles under one
    // account, and no separate "add another driver" path here either.
    // ---------------------------------------------------------------

    private Task StartRegisterDriverAsync(BotSession session, CancellationToken cancellationToken)
    {
        session.ResetOrder();
        session.Step = ConversationStep.AwaitingDriverName;
        return Reply(session, "driverAskName", cancellationToken);
    }

    private Task HandleDriverNameAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.NewDriverName = text;
        session.Step = ConversationStep.AwaitingDriverPhone;
        return Reply(session, "askPhone", cancellationToken);
    }

    private Task HandleDriverPhoneAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.NewDriverPhone = text;
        session.Step = ConversationStep.AwaitingDriverPlate;
        return Reply(session, "driverAskPlate", cancellationToken);
    }

    private Task HandleDriverPlateAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.NewDriverPlate = text;
        session.Step = ConversationStep.AwaitingDriverTruckType;
        return Reply(session, "driverAskTruckType", cancellationToken);
    }

    private static readonly string[] TruckTypeOptions =
        { "Pickup", "SmallTruck", "MediumTruck", "Isuzu", "HeavyTruck", "Trailer", "Other" };

    private async Task HandleDriverTruckTypeAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, out var number) || number < 1 || number > TruckTypeOptions.Length)
        {
            await Reply(session, "invalidTruckTypeNumber", cancellationToken);
            return;
        }

        session.Step = ConversationStep.Idle;
        var truckType = TruckTypeOptions[number - 1];
        var name = session.NewDriverName!;
        var phone = session.NewDriverPhone;
        var plate = session.NewDriverPlate;
        session.NewDriverName = null;
        session.NewDriverPhone = null;
        session.NewDriverPlate = null;

        try
        {
            await _fhsms.RegisterDriverProfileAsync(session.AgentToken!, name, phone, plate, truckType, cancellationToken);
            await Reply(session, "driverRegistered", cancellationToken, name, plate ?? "-", truckType);
        }
        catch (FhsmsApiException ex)
        {
            await Reply(session, "actionFailed", cancellationToken, ex.Message);
        }
    }

    // ---------------------------------------------------------------
    // Driver: browse and accept a trip (/trips). Same "reply with a number"
    // pattern as every other list-then-pick flow in this bot.
    // ---------------------------------------------------------------

    private async Task StartTripsAsync(BotSession session, CancellationToken cancellationToken)
    {
        var profile = await _fhsms.GetMyDriverProfileAsync(session.AgentToken!, cancellationToken);
        if (profile is null)
        {
            await Reply(session, "driverProfileRequired", cancellationToken);
            return;
        }

        var trips = await _fhsms.GetAvailableTripsAsync(session.AgentToken!, cancellationToken);
        session.ResetOrder();

        if (trips.Count == 0)
        {
            await Reply(session, "noTripsAvailable", cancellationToken);
            return;
        }

        session.LastShownTrips = trips;
        session.Step = ConversationStep.AwaitingTripSelection;
        await Reply(session, "chooseTrip", cancellationToken, FormatTripList(trips));
    }

    private async Task HandleTripSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, out var number) || number < 1 || number > session.LastShownTrips.Count)
        {
            await Reply(session, "invalidTripNumber", cancellationToken);
            return;
        }

        var trip = session.LastShownTrips[number - 1];
        session.Step = ConversationStep.Idle;

        try
        {
            await _fhsms.AcceptTripAsync(session.AgentToken!, trip.DeliveryId, cancellationToken);
            await Reply(session, "tripAccepted", cancellationToken, trip.OrderNumber, trip.DestinationName);
        }
        catch (FhsmsApiException ex)
        {
            await Reply(session, "actionFailed", cancellationToken, ex.Message);
        }
    }

    private async Task HandleMyTripsAsync(BotSession session, CancellationToken cancellationToken)
    {
        var trips = await _fhsms.GetMyTripsAsync(session.AgentToken!, cancellationToken);
        if (trips.Count == 0)
        {
            await Reply(session, "noTripsYet", cancellationToken);
            return;
        }
        await Reply(session, "myTripsHeader", cancellationToken, FormatTripList(trips, includeStatus: true));
    }

    // ---------------------------------------------------------------
    // Driver: confirm handoff on an accepted trip (/deliver). Only offers
    // trips the driver has actually accepted and that aren't delivered yet -
    // no signature/photo capture over chat (that stays a web-app extra);
    // this records the minimum a proof-of-delivery needs.
    // ---------------------------------------------------------------

    private async Task StartDeliverAsync(BotSession session, CancellationToken cancellationToken)
    {
        var trips = await _fhsms.GetMyTripsAsync(session.AgentToken!, cancellationToken);
        var deliverable = trips.Where(t => t.Status is "Pending" or "InTransit").ToList();

        session.ResetOrder();

        if (deliverable.Count == 0)
        {
            await Reply(session, "noTripsToDeliver", cancellationToken);
            return;
        }

        session.LastShownTrips = deliverable;
        session.Step = ConversationStep.AwaitingDeliverTripSelection;
        await Reply(session, "chooseTripToDeliver", cancellationToken, FormatTripList(deliverable, includeStatus: true));
    }

    private async Task HandleDeliverTripSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, out var number) || number < 1 || number > session.LastShownTrips.Count)
        {
            await Reply(session, "invalidTripNumber", cancellationToken);
            return;
        }

        session.SelectedTripForDelivery = session.LastShownTrips[number - 1];
        session.Step = ConversationStep.AwaitingDeliverReceivedByName;
        await Reply(session, "askReceivedByName", cancellationToken);
    }

    private async Task HandleDeliverReceivedByNameAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        var trip = session.SelectedTripForDelivery!;
        session.Step = ConversationStep.Idle;
        session.SelectedTripForDelivery = null;

        try
        {
            await _fhsms.MarkTripDeliveredAsync(session.AgentToken!, trip.DeliveryId, text, "Delivered via Telegram", cancellationToken);
            await Reply(session, "tripDelivered", cancellationToken, trip.OrderNumber);
        }
        catch (FhsmsApiException ex)
        {
            await Reply(session, "actionFailed", cancellationToken, ex.Message);
        }
    }

    private static string FormatTripList(List<TripDto> trips, bool includeStatus = false)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < trips.Count; i++)
        {
            var t = trips[i];
            var statusSuffix = includeStatus ? $" [{t.Status}]" : "";
            var priceSuffix = t.TripPrice is { } price ? $" - {price:0.##} ETB" : "";
            sb.AppendLine($"{i + 1}. {t.OriginLocation ?? "-"} -> {t.DestinationName}{priceSuffix}{statusSuffix}");
            sb.AppendLine($"   {t.ProductSummary} ({t.OrderNumber})");
        }
        return sb.ToString();
    }

    // ---------------------------------------------------------------
    // Hotel customer self-service: /register replaces the old
    // "ask your admin for a numeric customer ID, then type it in" flow -
    // a hotel customer just gives their name and phone, same as filling in
    // the web app's "New customer" form, and the bot creates the account
    // for them right there.
    // ---------------------------------------------------------------

    private Task StartCustomerRegistrationAsync(BotSession session, CancellationToken cancellationToken)
    {
        session.ResetOrder();
        session.Step = ConversationStep.AwaitingNewHotelName; // reuses the same 3-question flow as agent hotel registration
        return Reply(session, "registerAskName", cancellationToken);
    }

    private Task HandleNewHotelNameAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.NewHotelName = text;
        session.Step = ConversationStep.AwaitingNewHotelPhone;
        return Reply(session, "askPhone", cancellationToken);
    }

    private Task HandleNewHotelPhoneAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.NewHotelPhone = text;
        session.Step = ConversationStep.AwaitingNewHotelAddress;
        return Reply(session, "askAddress", cancellationToken);
    }

    private async Task HandleNewHotelAddressAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.Step = ConversationStep.Idle;
        var name = session.NewHotelName!;
        var phone = session.NewHotelPhone;

        try
        {
            if (session.IsAgentLoggedIn && session.AgentRole == "HotelAgent")
            {
                // A hotel agent registering a hotel on the client's behalf - uses the agent's own token.
                var id = await _fhsms.CreateCustomerAsync(session.AgentToken!, new CreateCustomerRequest
                {
                    Name = name, Phone = phone, Address = text
                }, cancellationToken);
                session.NewHotelName = null;
                session.NewHotelPhone = null;
                await Reply(session, "newHotelCreated", cancellationToken, name, id);
            }
            else
            {
                // A hotel customer self-registering - uses the service account, and immediately links this chat as that customer so /order works right away.
                var id = await _fhsms.CreateCustomerAsync(null, new CreateCustomerRequest
                {
                    Name = name, Phone = phone, Address = text
                }, cancellationToken);
                session.NewHotelName = null;
                session.NewHotelPhone = null;
                session.CustomerId = id;
                await Reply(session, "registered", cancellationToken, name);
            }
        }
        catch (FhsmsApiException ex)
        {
            await Reply(session, "actionFailed", cancellationToken, ex.Message);
        }
    }

    private async Task HandleMyOrdersAsync(BotSession session, CancellationToken cancellationToken)
    {
        if (session.CustomerId is null)
        {
            await Reply(session, "notRegistered", cancellationToken);
            return;
        }

        var orders = await _fhsms.GetOrdersByCustomerAsync(null, session.CustomerId.Value, cancellationToken);
        if (orders.Count == 0)
        {
            await Reply(session, "myOrdersEmpty", cancellationToken);
            return;
        }

        var sb = new StringBuilder();
        foreach (var o in orders.Take(10))
        {
            sb.AppendLine($"{o.OrderNumber} - {o.Status} - {o.OrderDate:yyyy-MM-dd} - {o.Subtotal:0.##} ETB");
            sb.AppendLine($"  id: {o.Id}");
        }

        await Reply(session, "myOrdersHeader", cancellationToken, sb.ToString());
    }

    private async Task HandleOrderStatusAsync(BotSession session, string? arg, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(arg) || !Guid.TryParse(arg, out var orderId))
        {
            await Reply(session, "orderStatusUsage", cancellationToken);
            return;
        }

        var order = await _fhsms.GetOrderAsync(null, orderId, cancellationToken);

        // Refuse to reveal an order that doesn't belong to this chat's
        // registered customer, even if the order ID happens to be valid -
        // otherwise /orderstatus becomes an ID-enumeration information leak.
        if (order is null || session.CustomerId is null || order.CustomerId != session.CustomerId.Value)
        {
            await Reply(session, "orderStatusNotFound", cancellationToken);
            return;
        }

        // The truck side - where the order physically is right now - is a
        // separate lookup from the order's own workflow status above (see
        // DeliveryDto remarks). No delivery yet just means it hasn't been
        // posted for pickup - not an error.
        var delivery = await _fhsms.GetDeliveryByOrderAsync(null, orderId, cancellationToken);
        string deliveryBlock;
        if (delivery is null)
        {
            deliveryBlock = BotTranslations.Get(session.Language, "orderStatusNoDelivery");
        }
        else
        {
            var driverPart = delivery.DriverName ?? BotTranslations.Get(session.Language, "orderStatusNoDriverYet");
            deliveryBlock = string.Format(
                BotTranslations.Get(session.Language, "orderStatusDelivery"),
                delivery.Status, driverPart, delivery.VehicleInfo ?? "-");
        }

        await Reply(session, "orderStatusResult", cancellationToken,
            order.OrderNumber, order.Status, order.OrderDate.ToString("yyyy-MM-dd HH:mm"), order.Subtotal.ToString("0.##"), deliveryBlock);
    }

    private async Task StartOrderAsync(BotSession session, CancellationToken cancellationToken)
    {
        if (session.CustomerId is null)
        {
            await Reply(session, "notRegistered", cancellationToken);
            return;
        }

        var products = await _fhsms.GetProductsAsync(null, cancellationToken);
        if (products.Count == 0)
        {
            await Reply(session, "noProducts", cancellationToken);
            return;
        }

        session.ResetOrder();
        session.LastShownProducts = products;
        session.Step = ConversationStep.AwaitingProductSelection;

        var listing = FormatProductList(products);
        await Reply(session, "chooseProduct", cancellationToken, listing);
    }

    private async Task HandleProductSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            || number < 1 || number > session.LastShownProducts.Count)
        {
            await Reply(session, "invalidProductNumber", cancellationToken);
            return;
        }

        session.PendingProduct = session.LastShownProducts[number - 1];
        session.Step = ConversationStep.AwaitingQuantity;
        await Reply(session, "askQuantity", cancellationToken, session.PendingProduct.Name);
    }

    private async Task HandleQuantityAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0)
        {
            await Reply(session, "invalidQuantity", cancellationToken);
            return;
        }

        var product = session.PendingProduct!;
        session.Cart.Add(new CartItem(product.Id, product.Name, product.UnitPrice ?? 0m, quantity));
        session.PendingProduct = null;
        session.Step = ConversationStep.AwaitingProductSelection;

        await Reply(session, "itemAdded", cancellationToken, quantity, product.Name);
    }

    private async Task HandleDoneAsync(BotSession session, CancellationToken cancellationToken)
    {
        if (session.Step is ConversationStep.AwaitingStockProductSelection or ConversationStep.AwaitingStockConfirmation)
        {
            await Reply(session, "stockCartEmpty", cancellationToken); // nudge towards the right command instead of misreading the stock cart as an order
            return;
        }

        if (session.Cart.Count == 0)
        {
            await Reply(session, "cartEmpty", cancellationToken);
            return;
        }

        session.Step = ConversationStep.AwaitingConfirmation;

        var sb = new StringBuilder();
        decimal total = 0;
        foreach (var item in session.Cart)
        {
            sb.AppendLine($"- {item.ProductName} x {item.Quantity} = {item.LineTotal:0.##} ETB");
            total += item.LineTotal;
        }

        await Reply(session, "orderSummary", cancellationToken, sb.ToString(), total.ToString("0.##", CultureInfo.InvariantCulture));
    }

    private async Task HandleConfirmAsync(BotSession session, CancellationToken cancellationToken)
    {
        if (session.Step != ConversationStep.AwaitingConfirmation || session.Cart.Count == 0 || session.CustomerId is null)
        {
            await Reply(session, "orderConfirmedFallback", cancellationToken);
            return;
        }

        try
        {
            var request = new CreateOrderRequest
            {
                CustomerId = session.CustomerId!.Value,
                Source = "Telegram",
                AgentUserId = null,
                Items = session.Cart
                    .Select(c => new CreateOrderItemRequest { ProductId = c.ProductId, Quantity = c.Quantity })
                    .ToList()
            };

            var orderId = await _fhsms.CreateOrderAsync(null, request, cancellationToken);
            session.ResetOrder();
            await Reply(session, "orderPlaced", cancellationToken, orderId);
        }
        catch (FhsmsApiException ex)
        {
            await Reply(session, "orderFailed", cancellationToken, ex.Message);
        }
    }

    // ---------------------------------------------------------------
    // Hotel agent: register a new hotel (/newhotel)
    // ---------------------------------------------------------------

    private Task StartNewHotelAsync(BotSession session, CancellationToken cancellationToken)
    {
        session.ResetOrder();
        session.Step = ConversationStep.AwaitingNewHotelName;
        return Reply(session, "newHotelAskName", cancellationToken);
    }

    // ---------------------------------------------------------------
    // Hotel agent: log an order for a hotel (/neworder). Reuses the same
    // "pick a product number, give a quantity, repeat, /done, /confirm"
    // pattern the customer flow uses, so an agent only has to learn it once.
    // ---------------------------------------------------------------

    private async Task StartAgentOrderAsync(BotSession session, CancellationToken cancellationToken)
    {
        var hotels = await _fhsms.GetCustomersAsync(session.AgentToken!, cancellationToken);
        hotels = hotels.Where(h => h.IsActive).ToList();

        session.ResetOrder();

        if (hotels.Count == 0)
        {
            await Reply(session, "noHotelsYet", cancellationToken);
            return;
        }

        session.LastShownHotels = hotels;
        session.Step = ConversationStep.AwaitingOrderHotelSelection;
        await Reply(session, "chooseHotel", cancellationToken, FormatHotelList(hotels));
    }

    private async Task HandleOrderHotelSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            || number < 1 || number > session.LastShownHotels.Count)
        {
            await Reply(session, "invalidHotelNumber", cancellationToken);
            return;
        }

        session.SelectedHotel = session.LastShownHotels[number - 1];

        var products = await _fhsms.GetProductsAsync(session.AgentToken, cancellationToken);
        if (products.Count == 0)
        {
            await Reply(session, "noProducts", cancellationToken);
            session.ResetOrder();
            return;
        }

        session.LastShownProducts = products;
        session.Step = ConversationStep.AwaitingOrderProductSelection;
        await Reply(session, "chooseProduct", cancellationToken, FormatProductList(products));
    }

    private async Task HandleOrderProductSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            || number < 1 || number > session.LastShownProducts.Count)
        {
            await Reply(session, "invalidProductNumber", cancellationToken);
            return;
        }

        session.PendingProduct = session.LastShownProducts[number - 1];
        session.Step = ConversationStep.AwaitingOrderQuantity;
        await Reply(session, "askQuantity", cancellationToken, session.PendingProduct.Name);
    }

    private async Task HandleOrderQuantityAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0)
        {
            await Reply(session, "invalidQuantity", cancellationToken);
            return;
        }

        var product = session.PendingProduct!;
        session.Cart.Add(new CartItem(product.Id, product.Name, product.UnitPrice ?? 0m, quantity));
        session.PendingProduct = null;
        session.Step = ConversationStep.AwaitingOrderProductSelection;

        await Reply(session, "itemAddedThenDone", cancellationToken, quantity, product.Name);
    }

    /// <summary>/done inside the agent order flow moves to confirmation - dispatched from HandleDoneAsync when session.Step is one of the order-entry steps.</summary>
    private Task HandleAgentOrderDoneAsync(BotSession session, CancellationToken cancellationToken)
    {
        if (session.Cart.Count == 0)
        {
            return Reply(session, "cartEmpty", cancellationToken);
        }

        session.Step = ConversationStep.AwaitingOrderConfirmation;

        var sb = new StringBuilder();
        decimal total = 0;
        foreach (var item in session.Cart)
        {
            sb.AppendLine($"- {item.ProductName} x {item.Quantity} = {item.LineTotal:0.##} ETB");
            total += item.LineTotal;
        }

        return Reply(session, "orderSummary", cancellationToken, sb.ToString(), total.ToString("0.##", CultureInfo.InvariantCulture));
    }

    private async Task HandleAgentOrderConfirmAsync(BotSession session, CancellationToken cancellationToken)
    {
        try
        {
            var request = new CreateOrderRequest
            {
                CustomerId = session.SelectedHotel!.Id,
                Source = "Telegram",
                AgentUserId = null, // server derives this from the agent's own token
                Items = session.Cart
                    .Select(c => new CreateOrderItemRequest { ProductId = c.ProductId, Quantity = c.Quantity })
                    .ToList()
            };

            var orderId = await _fhsms.CreateOrderAsync(session.AgentToken, request, cancellationToken);
            session.ResetOrder();
            await Reply(session, "orderPlaced", cancellationToken, orderId);
        }
        catch (FhsmsApiException ex)
        {
            await Reply(session, "orderFailed", cancellationToken, ex.Message);
        }
    }

    // ---------------------------------------------------------------
    // Farmer agent: register a new farmer (/newfarmer)
    // ---------------------------------------------------------------

    private Task StartNewFarmerAsync(BotSession session, CancellationToken cancellationToken)
    {
        session.ResetOrder();
        session.Step = ConversationStep.AwaitingNewFarmerName;
        return Reply(session, "newFarmerAskName", cancellationToken);
    }

    private Task HandleNewFarmerNameAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.NewFarmerName = text;
        session.Step = ConversationStep.AwaitingNewFarmerPhone;
        return Reply(session, "askPhone", cancellationToken);
    }

    private Task HandleNewFarmerPhoneAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.NewFarmerPhone = text;
        session.Step = ConversationStep.AwaitingNewFarmerLocation;
        return Reply(session, "askLocation", cancellationToken);
    }

    private async Task HandleNewFarmerLocationAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        session.Step = ConversationStep.Idle;
        var name = session.NewFarmerName!;
        var phone = session.NewFarmerPhone;
        session.NewFarmerName = null;
        session.NewFarmerPhone = null;

        try
        {
            var id = await _fhsms.CreateFarmerAsync(session.AgentToken!, new CreateFarmerRequest
            {
                Name = name, Phone = phone, Location = text
            }, cancellationToken);
            await Reply(session, "newFarmerCreated", cancellationToken, name, id);
        }
        catch (FhsmsApiException ex)
        {
            await Reply(session, "actionFailed", cancellationToken, ex.Message);
        }
    }

    // ---------------------------------------------------------------
    // Farmer agent: log stock received (/stockin). Same
    // pick-a-farmer/pick-a-product/give-a-quantity/repeat/confirm pattern -
    // one farmer agent can log several products from the same farmer in one
    // go (e.g. tomatoes and onions in the same delivery).
    // ---------------------------------------------------------------

    private async Task StartStockInAsync(BotSession session, CancellationToken cancellationToken)
    {
        var farmers = await _fhsms.GetFarmersAsync(session.AgentToken!, cancellationToken);
        farmers = farmers.Where(f => f.IsActive).ToList();

        session.ResetOrder();

        if (farmers.Count == 0)
        {
            await Reply(session, "noFarmersYet", cancellationToken);
            return;
        }

        session.LastShownFarmers = farmers;
        session.Step = ConversationStep.AwaitingStockFarmerSelection;
        await Reply(session, "chooseFarmer", cancellationToken, FormatFarmerList(farmers));
    }

    private async Task HandleStockFarmerSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            || number < 1 || number > session.LastShownFarmers.Count)
        {
            await Reply(session, "invalidFarmerNumber", cancellationToken);
            return;
        }

        session.SelectedFarmer = session.LastShownFarmers[number - 1];

        var products = await _fhsms.GetProductsAsync(session.AgentToken, cancellationToken);
        if (products.Count == 0)
        {
            await Reply(session, "noProducts", cancellationToken);
            session.ResetOrder();
            return;
        }

        session.LastShownProducts = products;
        session.Step = ConversationStep.AwaitingStockProductSelection;
        await Reply(session, "chooseProduct", cancellationToken, FormatProductList(products));
    }

    private async Task HandleStockProductSelectionAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            || number < 1 || number > session.LastShownProducts.Count)
        {
            await Reply(session, "invalidProductNumber", cancellationToken);
            return;
        }

        session.StockProduct = session.LastShownProducts[number - 1];
        session.Step = ConversationStep.AwaitingStockQuantity;
        await Reply(session, "askStockQuantity", cancellationToken,
            session.StockProduct.Name, session.StockProduct.UnitAbbreviation ?? "");
    }

    private async Task HandleStockQuantityAsync(BotSession session, string text, CancellationToken cancellationToken)
    {
        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0)
        {
            await Reply(session, "invalidQuantity", cancellationToken);
            return;
        }

        var product = session.StockProduct!;
        session.Cart.Add(new CartItem(product.Id, product.Name, product.UnitPrice ?? 0m, quantity));
        session.StockProduct = null;
        session.Step = ConversationStep.AwaitingStockProductSelection;

        await Reply(session, "stockItemAdded", cancellationToken, quantity, product.UnitAbbreviation ?? "", product.Name);
    }

    private async Task HandleStockDoneAsync(BotSession session, CancellationToken cancellationToken)
    {
        if (session.Step != ConversationStep.AwaitingStockProductSelection || session.Cart.Count == 0)
        {
            await Reply(session, "stockCartEmpty", cancellationToken);
            return;
        }

        session.Step = ConversationStep.AwaitingStockConfirmation;

        var sb = new StringBuilder();
        decimal totalOwed = 0m;
        foreach (var item in session.Cart)
        {
            var lineCost = item.UnitPrice * item.Quantity;
            totalOwed += lineCost;
            sb.AppendLine($"- {item.ProductName}: {item.Quantity} x {item.UnitPrice:0.##} = {lineCost:0.##} ETB");
        }

        // Preview the same figure the backend will freeze onto the
        // auto-generated FarmerInvoice for this stock-in (Quantity x buying
        // price at the moment RecordStockReceiptAsync is called below) - so
        // the agent sees what the farmer will be owed before confirming,
        // even though they never type a cost themselves.
        await Reply(session, "stockSummary", cancellationToken, session.SelectedFarmer?.Name ?? "", sb.ToString(), totalOwed);
    }

    private async Task HandleStockConfirmAsync(BotSession session, CancellationToken cancellationToken)
    {
        if (session.Step != ConversationStep.AwaitingStockConfirmation || session.Cart.Count == 0)
        {
            await Reply(session, "stockConfirmedFallback", cancellationToken);
            return;
        }

        try
        {
            Guid? lastId = null;
            foreach (var item in session.Cart)
            {
                lastId = await _fhsms.RecordStockReceiptAsync(session.AgentToken!, new RecordStockReceiptRequest
                {
                    ProductId = item.ProductId,
                    Type = "Receiving",
                    Quantity = item.Quantity,
                    FarmerId = session.SelectedFarmer!.Id,
                    Reference = $"Telegram / {session.SelectedFarmer.Name}"
                }, cancellationToken);
            }

            session.ResetOrder();
            await Reply(session, "stockRecorded", cancellationToken, lastId);
        }
        catch (FhsmsApiException ex)
        {
            await Reply(session, "actionFailed", cancellationToken, ex.Message);
        }
    }

    private static string FormatProductList(List<ProductDto> products)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < products.Count; i++)
        {
            var p = products[i];
            sb.AppendLine($"{i + 1}. {p.Name} - {p.UnitPrice ?? 0m:0.##} ETB{(p.UnitAbbreviation is null ? "" : $" / {p.UnitAbbreviation}")}");
        }
        return sb.ToString();
    }

    private static string FormatHotelList(List<CustomerDto> hotels)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < hotels.Count; i++)
        {
            var h = hotels[i];
            sb.AppendLine($"{i + 1}. {h.Name}{(h.Address is null ? "" : $" - {h.Address}")}");
        }
        return sb.ToString();
    }

    private static string FormatFarmerList(List<FarmerDto> farmers)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < farmers.Count; i++)
        {
            var f = farmers[i];
            sb.AppendLine($"{i + 1}. {f.Name}{(f.Location is null ? "" : $" - {f.Location}")}");
        }
        return sb.ToString();
    }

    private Task Reply(BotSession session, string key, CancellationToken cancellationToken, params object?[] args)
    {
        var template = BotTranslations.Get(session.Language, key);
        var text = args.Length > 0 ? string.Format(template, args) : template;
        return _telegram.SendMessageAsync(session.ChatId, text, cancellationToken);
    }
}
