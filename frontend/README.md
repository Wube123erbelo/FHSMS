# FHSMS Frontend

A Vite + React + TypeScript + Tailwind PWA for FHSMS, bilingual in English and
Amharic. This talks to `FHSMS.API` (see `../README.md` for backend setup) -
nothing here duplicates business logic; every page is a thin view over the
API's commands and queries.

## Running it

```bash
cd frontend
cp .env.example .env      # point VITE_API_BASE_URL at your running API
npm install
npm run dev
```

Sign in with the seeded admin: `admin@fhsms.local` / `ChangeMe123!`.

## Bilingual interface (English / Amharic)

- `src/i18n/en.ts` and `src/i18n/am.ts` are parallel translation dictionaries,
  grouped by page/area (`nav.*`, `tax.*`, `orders.*`, etc).
- `src/i18n/LanguageContext.tsx` exposes a typed `useTranslation()` hook:
  `const { t, language, setLanguage } = useTranslation();` - `t("tax.title")`
  is type-checked against the dictionary shape, so a typo'd key is a compile
  error, not a silent blank string.
- If a key is missing from `am.ts`, `t()` falls back to English rather than
  showing a raw key - translation work can always be partial without breaking
  the UI.
- The language toggle lives in the sidebar (`LanguageSwitcher.tsx`) and on the
  login screen; the choice persists to `localStorage` under `fhsms.language`.
- Amharic (Ge'ez script) reads left-to-right like English, so no RTL layout
  handling was needed - if a future language requires RTL, add a `dir`
  attribute driven by `language` in `Layout.tsx` and `LoginPage.tsx`.
- Coverage: the sidebar/navigation, login, dashboard, and every page's
  headers, buttons, table columns, and form labels are translated. Long
  free-text explanations (e.g. the "how tax flows" paragraph) are translated
  in full too. Things intentionally left in English: raw enum values sent by
  the API (e.g. `HotelAgent`, `Cash`, `Pending`) and placeholder/hint text in
  a couple of still-being-built forms - wire these into the dictionaries the
  same way as everything else when you get to them.

To add a new string: add the same key to both `en.ts` and `am.ts`, then call
`t("section.key")`. To add a third language: create `src/i18n/xx.ts`, add it
to the `DICTIONARIES` map in `LanguageContext.tsx`, and add an option to
`LanguageSwitcher.tsx`.

## What's built

Every backend module has a working page: Dashboard, Products, Customers,
Orders (+ detail, with invoice generation), Invoices (+ detail, showing the
frozen tax snapshot per line), Inventory, Deliveries, Commissions, Bank
Reconciliation, Notifications, Audit Logs, and the Tax & VAT settings screen
(the centerpiece - toggle, schedule rate changes, view history).

## Known gaps (mirrors backend README)

Several list endpoints don't exist on the backend yet (`GET /api/orders`,
`GET /api/invoices`, `GET /api/customers`), so those pages currently work by
ID lookup rather than a browsable table. Add the corresponding
`GetXQuery`/endpoint on the backend (same pattern as `GetProductsQuery`) and
swap the page over to `useFetch` the same way `ProductsPage` does.

## Design notes

The palette (evergreen/wheat/clay) and type pairing (Fraunces for display,
Inter for UI, IBM Plex Mono for codes/IDs) is a deliberate choice for a
farm-and-hotel operations tool, not a generic SaaS default. See
`tailwind.config.js` for the token definitions.

## PWA

`vite-plugin-pwa` is configured in `vite.config.ts` with a manifest and
auto-updating service worker. The icons in `public/` are solid-color
placeholders - replace `icon-192.png` and `icon-512.png` with real artwork
before shipping.
