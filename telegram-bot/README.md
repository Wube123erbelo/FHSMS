# FHSMS Telegram Bot

A standalone .NET console service that serves two audiences on the same
Telegram bot:

1. **Hotel and farmer agents (field students)** - the bot's primary purpose.
   An agent signs in with `/login` using the email/password an admin issued
   them (see the Users page or `POST /api/users/{id}/reset-password`), then
   registers hotels/farmers and logs orders/stock receipts from the field,
   even with a weak connection, since Telegram only needs a thin data
   connection to work. Every action is attributed to that specific agent
   server-side (from their JWT, not anything the bot sends), so commissions
   accrue correctly and automatically.
2. **Hotel/restaurant customers** ordering for themselves, no agent involved -
   the original self-service flow, now with a much simpler `/register` (see
   below).

It talks to the real Telegram Bot API directly (long polling - no public
webhook URL needed) and to the FHSMS backend as an authenticated client,
using the exact same commands (`CreateOrderCommand`, `CreateCustomerCommand`,
`CreateFarmerCommand`, `RecordInventoryTransactionCommand`) the web PWA uses.

## Setup

### 1. Get a bot token
Message [@BotFather](https://t.me/BotFather) on Telegram, run `/newbot`,
follow the prompts, and copy the token it gives you.

### 2. Configure
```bash
cd telegram-bot/src
dotnet user-secrets init
dotnet user-secrets set "Telegram:BotToken" "<token from BotFather>"
dotnet user-secrets set "Fhsms:ServiceAccountEmail" "admin@fhsms.local"
dotnet user-secrets set "Fhsms:ServiceAccountPassword" "ChangeMe123!"
```
`Fhsms:ApiBaseUrl` defaults to `http://localhost:5080/api` in
`appsettings.json` - override it the same way if your API runs elsewhere.

The service account is only used for the anonymous hotel-customer
self-ordering path (`GET /api/products`, `POST /api/customers`,
`POST /api/orders`) - any active FHSMS user works; a dedicated non-admin
account is a good idea for anything beyond local testing. Agents never use
this account - they authenticate as themselves via `/login`.

### 3. Run
```bash
dotnet run
```
You should see `Logged in. Polling Telegram for updates...`. Message your
bot on Telegram to try it.

## Conversation flow

### Hotel/restaurant customers (no login needed)
```
/start                       -> welcome message
/register                    -> guided: hotel name -> phone -> address, creates the account and links this chat to it
/order                       -> lists active products, numbered
  <number>                   -> pick a product
  <quantity>                 -> how many; item added to cart, loops back to product selection
/done                        -> shows cart + total, asks for confirmation
/confirm                     -> places the order via POST /api/orders (Source = Telegram)
/myorders / /orderstatus <id> -> check past orders
/cancel                      -> discards whatever is in progress
```
`/register` no longer requires an admin-issued customer ID - it's a guided
name/phone/address flow, same information the web app's "New customer" form
asks for, and it creates the account and links this chat to it immediately.

### Hotel and farmer agents
```
/login                        -> asks for email, then password (issued by an admin)
/logout                       -> signs out
/whoami                       -> shows who you're signed in as

# Hotel agent only:
/newhotel                     -> guided: name -> phone -> address
/neworder                     -> pick a hotel, then the same product/quantity loop as /order, then /done, /confirm

# Farmer agent only:
/newfarmer                    -> guided: name -> phone -> location
/stockin                      -> pick a farmer, then a product/quantity loop, then /stockdone, /stockconfirm
```
Every `/neworder` and `/stockin` call is made with the agent's own token, so
the backend can correctly credit their commission (2% of the invoice for
hotel agents, a configurable rate per unit for farmer agents - see the root
README's commission section) without the bot ever having to know or send an
agent ID itself.

If an account has two-factor authentication enabled, `/login` will tell the
user to sign in on the web app instead - the bot doesn't implement the TOTP
verification step.

## How it's wired to the backend

- `Clients/FhsmsApiClient.cs` sends an explicit bearer token on every call
  (an agent's own token from `/login`, or the service account's for the
  anonymous customer flow) rather than one shared default header - this
  matters because a single running instance of this class serves many
  concurrent chats with different logged-in identities at once.
- `Conversation/ConversationHandler.cs` has no order/registration/commission
  logic of its own - it only ever turns chat messages into the same request
  shapes the frontend sends, then relays the backend's response (or its
  validation error) back to the chat.
- `Clients/TelegramApiClient.cs` wraps `getUpdates`/`sendMessage` directly
  against `https://api.telegram.org` - no external Telegram library
  dependency, so there's nothing extra to audit or version-match.
- `Localization/BotTranslations.cs` mirrors the same English/Amharic split as
  `frontend/src/i18n` - same keys-and-fallback philosophy, just as a plain
  dictionary rather than a React context since this is a separate process.

## Known limitations

- **State is in-memory** (`Conversation/InMemoryBotStateStore.cs`), including
  an agent's login token: chat registrations, agent sessions, and
  in-progress carts are lost on restart, and this won't work correctly if
  you ever run more than one instance behind the same bot token (both
  instances would call `getUpdates` and race for updates). Implement
  `IBotStateStore` against a database table before doing either - and note
  an agent's token would need re-authenticating after any restart either way.
- **Long polling, not a webhook**: fine for one instance and low-to-moderate
  volume. For production/high-volume use, switch to Telegram's `setWebhook`
  and host an HTTP endpoint (e.g. add a minimal endpoint to `FHSMS.API` or a
  small ASP.NET Core app here) instead of the `getUpdates` loop in
  `Program.cs`.
- **No retry/backoff tuning beyond a flat 5-second pause** on polling errors.
- **No message-length handling**: Telegram caps messages at 4096 characters;
  a product/hotel/farmer catalog large enough to exceed that would need
  pagination in `ConversationHandler`'s `Format*List` helpers.
- **Passwords are typed in plain text in the Telegram chat** during `/login`
  - normal for bot-based auth, but worth telling agents their chat history
    contains their password, same as it would for any bot with a login step.

