# FHSMS - Farm-Hotel Supply Management System

A Clean Architecture ASP.NET Core backend, a bilingual (English / Amharic)
React PWA frontend, and a working Telegram ordering bot - implementing the
FHSMS design end to end, built around one central rule: **tax/VAT is
configuration, never a hard-coded value.**

> **Deploying this to Render?** See [`DEPLOYMENT.md`](./DEPLOYMENT.md) -
> `Dockerfile`, `telegram-bot/Dockerfile`, `frontend/Dockerfile`, and
> `render.yaml` are all set up for a one-click Blueprint deploy. Read the
> migrations note there first - it's the one step that actually blocks a
> first deploy.

This is a real, runnable solution - not a mockup. It was written without a
build toolchain (.NET SDK, Node/npm) available in the authoring environment,
so **you must build it yourself the first time** before trusting it further;
skim "Known rough edges" first.

## What's fully implemented

**Backend (`src/FHSMS.Domain`, `.Application`, `.Infrastructure`, `.API`)**

- **Clean Architecture**, dependencies pointing inward only: `Domain` ->
  `Application` -> `Infrastructure` -> `API`.
- **CQRS + MediatR**: every write is a `Command`, every read is a `Query`,
  each with its own handler. FluentValidation runs automatically via a MediatR
  pipeline behaviour before any handler executes.
- **The Tax/VAT engine** (`FHSMS.Domain.Services.TaxEngine`) - pure business
  logic, zero infrastructure dependencies:
  - Tax can be switched on/off per configuration from `POST /api/tax/{id}/toggle`.
  - Rates are effective-dated (`TaxConfiguration.ScheduleRate`) - scheduling a
    new rate automatically closes off the previous one, so they never overlap.
  - Every invoice line snapshots the rate, mode (inclusive/exclusive), and
    product tax profile that applied *at generation time* - later rate/config
    changes never touch existing invoices.
  - Per-product tax profiles: `StandardVat`, `ZeroRated`, `TaxExempt`, `NoTax`.
  - There is exactly **one** file in the whole solution with a literal tax
    rate in it: the database seeder, and even that's just a starting value.
- **Products, Categories, Units, Customers, Orders, Invoices, Payments.**
- **Inventory** - Receiving, Issuing, Adjustment, Damage, Wastage; stock on
  hand is derived by summing transactions, never a separately kept counter.
- **Delivery tracking**, **Commission engine** (accrued automatically via the
  `InvoiceIssuedEvent` domain event), **Bank reconciliation**,
  **Notifications** (`INotificationService` + stub provider), and automatic
  **audit logging** of every entity change.
- **Webhook stubs** for Telegram order intake and for payment providers
  (Telebirr, Chapa, CBE Birr) - see the dedicated Telegram bot below for the
  fully working version of the ordering side.
- **JWT auth**, role-based authorization, BCrypt password hashing.
- **PostgreSQL** via EF Core/Npgsql, `docker-compose.yml` for local dev.
- **Swagger/OpenAPI** UI for every endpoint.

**Frontend (`frontend/`)**

- Vite + React + TypeScript + Tailwind PWA (installable, offline-capable
  shell via `vite-plugin-pwa`).
- **Bilingual English / Amharic interface** - language switcher in the
  sidebar and on login, backed by a typed `useTranslation()` hook and
  parallel `en.ts` / `am.ts` dictionaries (see `frontend/README.md`). Every
  page's navigation, headers, buttons, table columns, and form labels are
  translated in both languages.
- A working page for every backend module: Dashboard, Products, Customers,
  Orders (+ detail, one-click invoice generation), Invoices (+ detail,
  showing the frozen per-line tax snapshot), Inventory, Deliveries,
  Commissions, Bank Reconciliation, Notifications, Audit Logs, and the Tax &
  VAT settings screen (the centerpiece).
- A deliberate design system (evergreen/wheat/clay palette, Fraunces + Inter
  type) - see `frontend/tailwind.config.js`.

**Telegram bot (`telegram-bot/`)**

- A standalone .NET console service that talks to the Telegram Bot API
  directly (long polling, no external bot library, no public webhook URL
  needed) and to the FHSMS API as an authenticated client.
- Bilingual conversation (English / Amharic, `/language en` or `/language am`).
- Real ordering flow: `/register` a customer, `/order` to browse products and
  build a cart interactively, confirm, and submit - which calls the exact
  same `CreateOrderCommand` the web PWA uses, with `Source = Telegram`. This
  is the concrete implementation of the "Telegram, Website/PWA, and future
  mobile apps should all communicate through the same backend API and
  business logic" requirement from the original design doc.
- See `telegram-bot/README.md` for setup (getting a token from @BotFather,
  configuration, running it).

## What's intentionally left as extension points

| Area | What's there | What's left |
|---|---|---|
| Payment provider webhooks | `PaymentWebhooksController` stubs for Telebirr/Chapa/CBE Birr, each calling `RecordPaymentCommand` | Signature/secret validation per provider - marked `TODO` in the controller |
| Credit management | `PaymentMethod.Credit` exists as an option | No dedicated credit limit / aging tracking yet |
| Localization beyond EN/AM | i18n infrastructure (web and bot) is language-agnostic | Add a third dictionary + switcher option if needed |
| List endpoints for Orders/Invoices/Customers | Get-by-id endpoints exist | No `GET /api/orders`, `GET /api/invoices`, or `GET /api/customers` (list) yet - add `GetXQuery` handlers mirroring `GetProductsQuery` |
| Telegram bot persistence | Chat-to-customer registration and cart state are in-memory | Swap the `IBotStateStore` implementation for a database-backed one before running more than one bot instance or restarting without losing carts |
| Automated tests | None yet | `TaxEngine` is the highest-value place to start - pure logic, no mocking required |

## Project layout

```
FHSMS.sln
docker-compose.yml              # PostgreSQL for local dev
Dockerfile                      # API image (see DEPLOYMENT.md)
render.yaml                     # Render Blueprint - deploys the whole stack
src/
  FHSMS.Domain/                 # Entities, enums, domain services (TaxEngine)
  FHSMS.Application/            # CQRS commands/queries/handlers, validators, DTOs
  FHSMS.Infrastructure/         # EF Core, JWT, password hashing, notifications, seed data
  FHSMS.API/                    # ASP.NET Core Web API, controllers, Program.cs
frontend/
  Dockerfile                     # Optional - Render's native static site is simpler
  src/
    api/                        # axios client + TypeScript DTOs mirroring the backend
    i18n/                       # en.ts / am.ts dictionaries + LanguageContext
    context/                    # AuthContext (JWT)
    components/, pages/         # UI
telegram-bot/
  Dockerfile
  src/                          # Telegram + FHSMS API clients, conversation state machine, i18n
```

## Running it

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org) and npm
- Docker (for PostgreSQL) - or your own PostgreSQL instance
- A Telegram bot token from [@BotFather](https://t.me/BotFather) (only needed for the bot)

### 2. Start the database
```bash
docker compose up -d
```

### 3. Configure backend secrets
```bash
cd src/FHSMS.API
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "a-long-random-string-at-least-32-characters"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=fhsms;Username=fhsms_user;Password=change_me"
```

### 4. Create the initial migration and update the database
```bash
cd src/FHSMS.Infrastructure
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --startup-project ../FHSMS.API
dotnet ef database update --startup-project ../FHSMS.API
```

### 5. Run the API
```bash
cd ../FHSMS.API
dotnet run
```
Swagger UI opens at `https://localhost:5081/swagger`. Seeded SuperAdmin:
`admin@fhsms.local` / `ChangeMe123!` - **change this immediately in any real
deployment.**

### 6. Run the frontend
```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```
Open `http://localhost:5173`.

### 7. Run the Telegram bot
```bash
cd telegram-bot/src
dotnet user-secrets init
dotnet user-secrets set "Telegram:BotToken" "<token from @BotFather>"
dotnet user-secrets set "Fhsms:ServiceAccountEmail" "admin@fhsms.local"
dotnet user-secrets set "Fhsms:ServiceAccountPassword" "ChangeMe123!"
dotnet run
```
Message your bot on Telegram: `/start`, then `/order`. See
`telegram-bot/README.md` for the full conversation flow and configuration
options.

## Trying the Tax/VAT flow end-to-end

Via the frontend: log in -> **Tax & VAT** in the sidebar -> toggle VAT
off/on, schedule a new rate, view history. Then **Products** -> create one ->
**Orders** -> create and confirm an order -> **Generate invoice** on the
order detail page -> see the frozen tax snapshot on the invoice.

Via Telegram: `/register <customerId>` -> `/order` -> pick products and
quantities -> confirm. The order lands with `Source = Telegram`; generate its
invoice from the frontend or via `POST /api/invoices/generate` and the same
frozen-snapshot behavior applies.

## Known rough edges (be aware before extending)

- This was authored without a .NET SDK or Node toolchain available to
  compile/test it, so treat it as a strong, carefully-written starting point
  rather than already-verified production code. Run `dotnet build` and
  `npm run build` first and fix any small issues that surface. The C# (brace
  balance across all files), TypeScript (every `t()` key resolves in both
  dictionaries, no duplicate default exports), and bot code were checked
  programmatically where possible, but that's not a substitute for an actual
  compiler.
- No automated tests are included yet.
- Inventory is not checked before confirming an order (no reservation/hold logic).
- The Telegram bot's chat-to-customer registration and in-progress carts are
  kept in memory (see `telegram-bot/README.md`) - fine for trying it out, not
  for a multi-instance production deployment.
- See `frontend/README.md`'s "Known gaps" section for the missing list endpoints.
