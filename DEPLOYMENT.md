# Deploying to Render

This repo can be deployed as three Render services sharing one managed
Postgres database: the API (Docker), the frontend (static site), and the
Telegram bot (Docker background worker). `render.yaml` in the repo root
defines all of it as a [Render Blueprint](https://render.com/docs/blueprint-spec).

## Before you deploy: generate the database migrations

**This is the one step that actually blocks a first deploy.** `Program.cs`
calls `Database.MigrateAsync()` on startup, which *applies* EF Core
migrations - it does not generate your schema from the model the way
`EnsureCreated()` would. This repo doesn't have a `Migrations/` folder
committed yet, so without this step the API will start successfully and
every table will be missing.

Locally, with a Postgres reachable (`docker compose up -d` starts one):

```bash
dotnet tool install --global dotnet-ef   # if you don't have it already
dotnet ef migrations add InitialCreate \
  --project src/FHSMS.Infrastructure \
  --startup-project src/FHSMS.API
```

Commit the generated `src/FHSMS.Infrastructure/Migrations/` folder. From then
on, every future schema change just needs a new `dotnet ef migrations add
<Name>` committed alongside it - Render applies them automatically on every
deploy via that same startup call.

**Also worth doing before your first deploy:** run `npm install` in
`frontend/` locally once and commit the resulting `package-lock.json`. There
isn't one in the repo yet, so `render.yaml` and `frontend/Dockerfile` both
use `npm install` instead of `npm ci` - that works, but means every build
picks up whatever the latest matching dependency versions are at build time
rather than a locked, reproducible set. Committing the lockfile and
switching both to `npm ci` afterward is a quick, worthwhile follow-up.

## Option A: one-click Blueprint deploy (recommended)

1. Push this repo to GitHub/GitLab.
2. In the Render dashboard: **New > Blueprint**, point it at your repo.
   Render reads `render.yaml` and shows you three services plus a database.
3. Click **Apply**. Render provisions the Postgres database first, then
   builds and deploys the API, frontend, and bot.
4. You'll be prompted to fill in the fields marked `sync: false` in
   `render.yaml` - these are secrets deliberately left out of the file:
   - `Telegram__BotToken` - from [@BotFather](https://t.me/BotFather)
   - `Fhsms__ServiceAccountEmail` / `Fhsms__ServiceAccountPassword` - an
     existing admin account the bot signs in as (the seeded default is
     `admin@fhsms.local` / `ChangeMe123!` - **change this password before
     going live**, then update the secret to match)
   - The four `PaymentProviders__*__Secret` values, once you have real
     webhook secrets from each provider
5. After the first deploy, check the actual `.onrender.com` URLs Render
   assigned (they usually match the `name:` fields in `render.yaml`, but
   confirm). If they differ, update `ALLOWED_ORIGINS` on the API service and
   `Fhsms__ApiBaseUrl` on the bot service to match, then redeploy those two.

## Option B: manual setup via the dashboard

If you'd rather not use the Blueprint, create each piece by hand:

- **Database**: New > PostgreSQL.
- **API**: New > Web Service > this repo > Docker runtime, Dockerfile path
  `./Dockerfile`, build context `.` (repo root). Set env vars listed below.
  Health check path: `/health`.
- **Frontend**: New > Static Site > this repo. Build command
  `cd frontend && npm install && npm run build`, publish directory
  `frontend/dist`. Add a rewrite rule `/*` → `/index.html` (Settings >
  Redirects/Rewrites) so React Router routes work on refresh. Set
  `VITE_API_BASE_URL` as an environment variable *at build time* - Render's
  static site builds do read env vars during the build step.
- **Telegram bot**: New > Background Worker > this repo > Docker runtime,
  Dockerfile path `./telegram-bot/Dockerfile`, build context `.`.

### Environment variables the API needs

| Variable | Notes |
|---|---|
| `DATABASE_URL` | Render's Postgres "Internal Connection String" (`postgres://...`). `Program.cs` parses this format automatically and converts it into the Npgsql connection string - no manual reformatting needed. |
| `Jwt__Secret` | Long random string. Render's `generateValue: true` (already set in `render.yaml`) handles this for you on Blueprint deploys. |
| `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpiryMinutes` | Match `appsettings.json`'s defaults unless you have a reason to change them. |
| `ALLOWED_ORIGINS` | Your frontend's real URL, e.g. `https://fhsms-frontend.onrender.com`. Comma-separate more than one. Left unset, CORS allows any origin - fine for a first deploy, tighten it once you know the real URL. |
| `PaymentProviders__BankTransfer__Secret`, `..Telebirr__Secret`, `..Chapa__Secret`, `..CbeBirr__Secret` | Only needed once you wire up real payment webhooks. |
| `PORT` | Set automatically by Render - `Program.cs` reads it and binds Kestrel to it. Don't set this yourself locally. |

### Environment variables the Telegram bot needs

| Variable | Notes |
|---|---|
| `Telegram__BotToken` | From [@BotFather](https://t.me/BotFather). |
| `Fhsms__ApiBaseUrl` | Your deployed API's URL plus `/api`, e.g. `https://fhsms-api.onrender.com/api`. |
| `Fhsms__ServiceAccountEmail`, `Fhsms__ServiceAccountPassword` | An existing admin/staff account the bot authenticates as. |

## Testing the Docker images locally first

Worth doing once before trusting a Render deploy - same images, faster
feedback loop:

```bash
# API (needs a reachable Postgres - docker compose up -d starts one)
docker build -t fhsms-api -f Dockerfile .
docker run --rm -p 8080:8080 \
  -e DATABASE_URL="postgres://fhsms_user:change_me@host.docker.internal:5432/fhsms" \
  -e Jwt__Secret="a-long-random-string-at-least-32-characters" \
  -e PORT=8080 \
  fhsms-api

# Telegram bot
docker build -t fhsms-bot -f telegram-bot/Dockerfile .
docker run --rm \
  -e Telegram__BotToken="<token>" \
  -e Fhsms__ApiBaseUrl="http://host.docker.internal:8080/api" \
  -e Fhsms__ServiceAccountEmail="admin@fhsms.local" \
  -e Fhsms__ServiceAccountPassword="ChangeMe123!" \
  fhsms-bot

# Frontend (optional - only if you're using frontend/Dockerfile instead of
# Render's native static site)
docker build -t fhsms-frontend -f frontend/Dockerfile \
  --build-arg VITE_API_BASE_URL="http://localhost:8080/api" .
docker run --rm -p 8081:80 -e PORT=80 fhsms-frontend
```

## What to double-check after deploying

- Hit `https://<your-api>.onrender.com/health` - should return
  `{"status":"healthy"}`.
- Log in to the frontend with the seeded admin (`admin@fhsms.local` /
  `ChangeMe123!`) and **change that password immediately** - it's a
  publicly-known default the moment this repo is public.
- Message your bot `/start` on Telegram and confirm it responds - if it
  doesn't, check the worker's logs in the Render dashboard for an auth error
  against `Fhsms__ServiceAccountEmail`/`Password`.
- Free-tier Render Postgres databases expire after 30 days and free web
  services spin down after 15 minutes of inactivity (adding a slow first
  request) - fine for evaluating this, not for real users. Upgrade the plans
  in the Render dashboard when you're ready to go live.
