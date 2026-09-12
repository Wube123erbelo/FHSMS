import { Link } from "react-router-dom";
import { useState } from "react";
import {
  Sprout, Menu, X, Truck, ShieldCheck, Tag, Users2, Building2,
  Package, Gauge, Facebook, Send, Linkedin, Music2, Mail, Phone, MapPin
} from "lucide-react";
import { useTranslation, type Language } from "../../i18n/LanguageContext";

export default function LandingPage() {
  const { t } = useTranslation();
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  return (
    <div className="min-h-screen bg-canvas">
      <SiteHeader mobileNavOpen={mobileNavOpen} setMobileNavOpen={setMobileNavOpen} />
      <Hero />
      <StatsBar />
      <FarmToTable />
      <WhyAgriLink />
      <SiteFooter />
    </div>
  );
}

// ---------------------------------------------------------------------------
// Header
// ---------------------------------------------------------------------------

function SiteHeader({ mobileNavOpen, setMobileNavOpen }: { mobileNavOpen: boolean; setMobileNavOpen: (v: boolean) => void }) {
  const { t } = useTranslation();

  const navLinks = [
    { href: "#home", label: t("landing.navHome") },
    { href: "#about", label: t("landing.navAbout") },
    { href: "#services", label: t("landing.navServices") },
    { href: "#why", label: t("landing.navWhy") },
    { href: "#contact", label: t("landing.navContact") }
  ];

  return (
    <header className="sticky top-0 z-30 border-b border-evergreen-100 bg-white/95 backdrop-blur">
      <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-4 py-3 sm:px-6">
        <a href="#home" className="flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-evergreen-700 text-white">
            <Sprout className="h-5 w-5" strokeWidth={1.75} />
          </span>
          <span className="font-display text-lg leading-none text-evergreen-900">
            AgriLink <span className="font-sans text-sm font-normal text-evergreen-600">Ethiopia</span>
          </span>
        </a>

        <nav className="hidden items-center gap-6 lg:flex">
          {navLinks.map((link) => (
            <a key={link.href} href={link.href} className="text-sm font-medium text-ink-600 transition hover:text-evergreen-700">
              {link.label}
            </a>
          ))}
        </nav>

        <div className="hidden items-center gap-3 lg:flex">
          <HeaderLanguageToggle />
          <Link to="/login" className="btn-primary">{t("landing.loginButton")}</Link>
        </div>

        <button className="lg:hidden" onClick={() => setMobileNavOpen(!mobileNavOpen)} aria-label="Menu">
          {mobileNavOpen ? <X className="h-6 w-6 text-evergreen-900" /> : <Menu className="h-6 w-6 text-evergreen-900" />}
        </button>
      </div>

      {mobileNavOpen && (
        <div className="border-t border-evergreen-100 bg-white px-4 py-3 lg:hidden">
          <nav className="flex flex-col gap-3">
            {navLinks.map((link) => (
              <a key={link.href} href={link.href} onClick={() => setMobileNavOpen(false)} className="text-sm font-medium text-ink-600">
                {link.label}
              </a>
            ))}
          </nav>
          <div className="mt-3 flex items-center justify-between gap-3 border-t border-evergreen-100 pt-3">
            <HeaderLanguageToggle />
            <Link to="/login" className="btn-primary">{t("landing.loginButton")}</Link>
          </div>
        </div>
      )}
    </header>
  );
}

function HeaderLanguageToggle() {
  const { language, setLanguage } = useTranslation();
  const options: { value: Language; label: string }[] = [{ value: "en", label: "En" }, { value: "am", label: "Am" }];

  return (
    <div className="flex overflow-hidden rounded-md border border-evergreen-200">
      {options.map((opt) => (
        <button
          key={opt.value}
          onClick={() => setLanguage(opt.value)}
          className={`px-2.5 py-1 text-xs font-medium transition ${
            language === opt.value ? "bg-evergreen-700 text-white" : "bg-white text-ink-600 hover:bg-evergreen-50"
          }`}
        >
          {opt.label}
        </button>
      ))}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Hero - the signature element: an illustrated terraced-field horizon with a
// delivery truck on the road and a few slow-drifting "produce" motifs. Built
// entirely from SVG/CSS (no stock photography), matching the brand palette
// already established across the app.
// ---------------------------------------------------------------------------

function Hero() {
  const { t } = useTranslation();

  return (
    <section id="home" className="relative overflow-hidden">
      <FieldScene />

      <div className="relative mx-auto max-w-7xl px-4 pb-16 pt-14 sm:px-6 sm:pb-24 sm:pt-20">
        <div className="max-w-2xl">
          <p className="mb-3 inline-block rounded-full bg-white/90 px-3 py-1 text-xs font-semibold uppercase tracking-widest text-evergreen-700 shadow-sm">
            {t("landing.heroEyebrow")}
          </p>
          <h1 className="font-display text-4xl leading-tight text-evergreen-950 drop-shadow-sm sm:text-5xl">
            {t("landing.heroTitleLine1")}
            <br />
            {t("landing.heroTitleLine2")}
          </h1>
          <p className="mt-4 max-w-xl text-base text-ink-600 sm:text-lg">
            {t("landing.heroSubtitle")}
          </p>
        </div>

        <div className="mt-8 grid max-w-2xl grid-cols-1 gap-4 sm:grid-cols-2">
          <PortalCard
            tone="farmer"
            icon={Sprout}
            title={t("landing.farmerPortalTitle")}
            subtitle={t("landing.farmerPortalSubtitle")}
            cta={t("landing.enterNow")}
          />
          <PortalCard
            tone="hotel"
            icon={Building2}
            title={t("landing.hotelPortalTitle")}
            subtitle={t("landing.hotelPortalSubtitle")}
            cta={t("landing.enterNow")}
          />
        </div>
      </div>
    </section>
  );
}

function PortalCard({
  tone, icon: Icon, title, subtitle, cta
}: { tone: "farmer" | "hotel"; icon: React.ElementType; title: string; subtitle: string; cta: string }) {
  const palette = tone === "farmer"
    ? { bg: "bg-evergreen-700", ring: "ring-evergreen-300", btn: "bg-evergreen-900 hover:bg-evergreen-800" }
    : { bg: "bg-[#2B4C6F]", ring: "ring-[#9CB8D4]", btn: "bg-[#1D3A57] hover:bg-[#16304A]" };

  return (
    <Link
      to="/login"
      className={`group relative overflow-hidden rounded-xl ${palette.bg} p-5 text-white shadow-lg ring-1 ${palette.ring} transition hover:-translate-y-0.5 hover:shadow-xl`}
    >
      <div className="mb-3 flex h-10 w-10 items-center justify-center rounded-lg bg-white/15">
        <Icon className="h-5 w-5" strokeWidth={1.75} />
      </div>
      <p className="font-display text-lg leading-tight">{title}</p>
      <p className="mt-1 text-xs text-white/75">{subtitle}</p>
      <span className={`mt-4 inline-flex items-center gap-1.5 rounded-md ${palette.btn} px-3 py-1.5 text-xs font-semibold transition`}>
        {cta} <span aria-hidden>&rarr;</span>
      </span>
    </Link>
  );
}

/** Layered terraced-field horizon, road with a driving truck, and slow-drifting produce motifs - pure SVG/CSS, no photography. */
function FieldScene() {
  return (
    <div className="absolute inset-0 -z-10 overflow-hidden bg-gradient-to-b from-wheat-50 via-evergreen-50 to-white">
      {/* Drifting ambient "produce" bubbles */}
      <span className="animate-float-slow absolute left-[8%] top-16 h-16 w-16 rounded-full bg-clay-400/25 blur-sm sm:h-24 sm:w-24" />
      <span className="animate-float-slower absolute right-[12%] top-28 h-10 w-10 rounded-full bg-wheat-400/30 blur-sm sm:h-16 sm:w-16" />
      <span className="animate-float-slow absolute left-[38%] top-4 h-8 w-8 rounded-full bg-evergreen-400/20 blur-sm" />
      <span className="animate-float-slower absolute right-[30%] top-10 h-6 w-6 rounded-full bg-clay-300/30 blur-sm" />

      {/* Terraced fields */}
      <svg viewBox="0 0 1440 420" preserveAspectRatio="none" className="absolute bottom-0 left-0 h-64 w-full sm:h-80">
        <path d="M0,180 L1440,120 L1440,420 L0,420 Z" fill="#BBD1C1" opacity="0.6" />
        <path d="M0,240 L1440,200 L1440,420 L0,420 Z" fill="#96B7A2" opacity="0.7" />
        <path d="M0,300 L1440,280 L1440,420 L0,420 Z" fill="#3C6B54" opacity="0.85" />
        <path d="M0,350 L1440,340 L1440,420 L0,420 Z" fill="#264A3A" />
      </svg>

      {/* Road + truck */}
      <svg viewBox="0 0 1440 100" preserveAspectRatio="none" className="absolute bottom-0 left-0 h-16 w-full sm:h-20">
        <rect x="0" y="55" width="1440" height="45" fill="#1C3B32" />
        <rect x="0" y="75" width="1440" height="4" fill="#EAD9A0" opacity="0.6" />
      </svg>
      <div className="animate-drive absolute bottom-2 h-10 w-16 sm:bottom-3 sm:h-14 sm:w-24">
        <TruckIcon />
      </div>
    </div>
  );
}

function TruckIcon() {
  return (
    <svg viewBox="0 0 64 40" className="h-full w-full drop-shadow-md">
      <rect x="2" y="8" width="34" height="20" rx="2" fill="#EEF3EF" stroke="#1C3B32" strokeWidth="1.5" />
      <rect x="6" y="12" width="26" height="10" rx="1" fill="#264A3A" />
      <path d="M36 16 H50 L58 24 V28 H36 Z" fill="#264A3A" stroke="#1C3B32" strokeWidth="1.5" />
      <rect x="40" y="19" width="8" height="6" rx="1" fill="#C9A227" />
      <circle cx="14" cy="30" r="5" fill="#12241E" />
      <circle cx="14" cy="30" r="2" fill="#D6E3D9" />
      <circle cx="48" cy="30" r="5" fill="#12241E" />
      <circle cx="48" cy="30" r="2" fill="#D6E3D9" />
    </svg>
  );
}

// ---------------------------------------------------------------------------
// Farm to table - hand-drawn-style SVG illustrations (harvest crate, delivery
// van, hotel storefront), matching FieldScene/TruckIcon exactly - no stock
// photography anywhere in this app, by design (see FieldScene remarks).
// ---------------------------------------------------------------------------

function ProduceCrateIllustration() {
  return (
    <svg viewBox="0 0 160 140" className="h-full w-full">
      {/* Wooden crate */}
      <rect x="14" y="78" width="132" height="52" rx="4" fill="#C9A227" stroke="#8A6E1A" strokeWidth="2" />
      <rect x="14" y="78" width="132" height="10" fill="#EAD9A0" opacity="0.6" />
      <line x1="14" y1="104" x2="146" y2="104" stroke="#8A6E1A" strokeWidth="2" opacity="0.5" />
      <line x1="40" y1="78" x2="40" y2="130" stroke="#8A6E1A" strokeWidth="2" opacity="0.4" />
      <line x1="120" y1="78" x2="120" y2="130" stroke="#8A6E1A" strokeWidth="2" opacity="0.4" />

      {/* Tomatoes */}
      <circle cx="42" cy="66" r="17" fill="#A64B3B" />
      <circle cx="42" cy="66" r="17" fill="url(#tomatoShine)" />
      <path d="M36 51 Q42 44 48 51" stroke="#3C6B54" strokeWidth="3" fill="none" strokeLinecap="round" />
      <circle cx="70" cy="70" r="13" fill="#B85B48" />
      <path d="M65 59 Q70 53 75 59" stroke="#3C6B54" strokeWidth="2.5" fill="none" strokeLinecap="round" />

      {/* Carrots */}
      <g>
        <path d="M98 82 L108 46 L114 48 L104 84 Z" fill="#D98C4A" />
        <path d="M104 84 L112 82 L108 96 Z" fill="#C97A38" />
        <path d="M105 46 Q108 36 111 45" stroke="#4C7A5E" strokeWidth="2.5" fill="none" strokeLinecap="round" />
        <path d="M109 47 Q113 39 116 47" stroke="#4C7A5E" strokeWidth="2.5" fill="none" strokeLinecap="round" />
      </g>

      {/* Leafy greens */}
      <g>
        <path d="M126 82 Q118 62 130 48 Q140 62 132 82 Z" fill="#4C7A5E" />
        <path d="M118 84 Q112 68 122 56 Q130 68 124 84 Z" fill="#5F8F6E" />
      </g>

      {/* Small mango accent */}
      <ellipse cx="24" cy="72" rx="10" ry="13" fill="#E0A93A" transform="rotate(-18 24 72)" />

      <defs>
        <radialGradient id="tomatoShine" cx="35%" cy="30%" r="60%">
          <stop offset="0%" stopColor="#fff" stopOpacity="0.35" />
          <stop offset="100%" stopColor="#fff" stopOpacity="0" />
        </radialGradient>
      </defs>
    </svg>
  );
}

function DeliveryVanIllustration() {
  return (
    <svg viewBox="0 0 160 140" className="h-full w-full">
      {/* Ground shadow + motion lines */}
      <ellipse cx="80" cy="118" rx="62" ry="6" fill="#264A3A" opacity="0.1" />
      <line x1="6" y1="60" x2="26" y2="60" stroke="#96B7A2" strokeWidth="3" strokeLinecap="round" opacity="0.7" />
      <line x1="2" y1="72" x2="20" y2="72" stroke="#96B7A2" strokeWidth="3" strokeLinecap="round" opacity="0.5" />

      {/* Van body */}
      <rect x="28" y="52" width="80" height="46" rx="6" fill="#EEF3EF" stroke="#1C3B32" strokeWidth="2" />
      <rect x="34" y="60" width="68" height="24" rx="2" fill="#264A3A" />
      {/* Cargo icon on side panel */}
      <rect x="52" y="66" width="16" height="12" rx="1.5" fill="#C9A227" />
      <path d="M52 70 H68 M60 66 V78" stroke="#8A6E1A" strokeWidth="1.2" />
      {/* Cab */}
      <path d="M108 62 H130 L142 76 V92 H108 Z" fill="#3C6B54" stroke="#1C3B32" strokeWidth="2" />
      <path d="M112 68 H128 L136 78 H112 Z" fill="#BBD1C1" opacity="0.85" />
      {/* Wheels */}
      <circle cx="52" cy="100" r="11" fill="#12241E" />
      <circle cx="52" cy="100" r="4.5" fill="#D6E3D9" />
      <circle cx="120" cy="100" r="11" fill="#12241E" />
      <circle cx="120" cy="100" r="4.5" fill="#D6E3D9" />
      {/* Headlight */}
      <circle cx="138" cy="84" r="3" fill="#EAD9A0" />
    </svg>
  );
}

function HotelIllustration() {
  return (
    <svg viewBox="0 0 160 140" className="h-full w-full">
      <ellipse cx="80" cy="126" rx="66" ry="6" fill="#264A3A" opacity="0.1" />

      {/* Building */}
      <rect x="26" y="46" width="108" height="76" fill="#EEF3EF" stroke="#1C3B32" strokeWidth="2" />
      <rect x="26" y="46" width="108" height="10" fill="#3C6B54" />

      {/* Awning */}
      <path d="M20 58 L140 58 L130 76 L30 76 Z" fill="#A64B3B" />
      <path d="M30 76 L38 76 L34 84 Z" fill="#8C3C2E" />
      <path d="M46 76 L54 76 L50 84 Z" fill="#8C3C2E" />
      <path d="M62 76 L70 76 L66 84 Z" fill="#8C3C2E" />
      <path d="M78 76 L86 76 L82 84 Z" fill="#8C3C2E" />
      <path d="M94 76 L102 76 L98 84 Z" fill="#8C3C2E" />
      <path d="M110 76 L118 76 L114 84 Z" fill="#8C3C2E" />
      <path d="M126 76 L134 76 L130 84 Z" fill="#8C3C2E" />

      {/* Windows */}
      <rect x="36" y="90" width="18" height="18" rx="1.5" fill="#264A3A" opacity="0.85" />
      <rect x="100" y="90" width="18" height="18" rx="1.5" fill="#264A3A" opacity="0.85" />
      <rect x="36" y="90" width="18" height="18" rx="1.5" fill="none" stroke="#1C3B32" strokeWidth="1.5" />
      <rect x="100" y="90" width="18" height="18" rx="1.5" fill="none" stroke="#1C3B32" strokeWidth="1.5" />

      {/* Door */}
      <rect x="68" y="88" width="24" height="34" rx="2" fill="#C9A227" stroke="#8A6E1A" strokeWidth="1.5" />
      <circle cx="86" cy="106" r="1.6" fill="#3B2E0E" />

      {/* Sign */}
      <rect x="58" y="30" width="44" height="14" rx="3" fill="#264A3A" />
      <circle cx="66" cy="37" r="2.4" fill="#EAD9A0" />
      <circle cx="74" cy="37" r="2.4" fill="#EAD9A0" opacity="0.6" />
    </svg>
  );
}

// ---------------------------------------------------------------------------
// Farm to table
// ---------------------------------------------------------------------------

function FarmToTable() {
  const { t } = useTranslation();

  const steps: { Illustration: React.ElementType; title: string; body: string }[] = [
    { Illustration: ProduceCrateIllustration, title: t("landing.journeyHarvestTitle"), body: t("landing.journeyHarvestBody") },
    { Illustration: DeliveryVanIllustration, title: t("landing.journeyDeliveryTitle"), body: t("landing.journeyDeliveryBody") },
    { Illustration: HotelIllustration, title: t("landing.journeyServedTitle"), body: t("landing.journeyServedBody") }
  ];

  return (
    <section className="bg-wheat-50/60 py-14">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <div className="mb-10 text-center">
          <h2 className="font-display text-2xl text-evergreen-900 sm:text-3xl">{t("landing.journeyHeading")}</h2>
          <p className="mt-2 text-sm text-ink-600">{t("landing.journeySubheading")}</p>
        </div>

        <div className="relative grid grid-cols-1 gap-8 sm:grid-cols-3">
          {/* Connecting path, desktop only */}
          <div className="pointer-events-none absolute inset-x-0 top-16 hidden border-t-2 border-dashed border-wheat-400 sm:block" />

          {steps.map((s, i) => (
            <div key={s.title} className="relative flex flex-col items-center text-center">
              <div className="relative z-10 mb-4 flex h-32 w-32 items-center justify-center rounded-full bg-white shadow-md ring-4 ring-wheat-50 sm:h-36 sm:w-36">
                <div className="h-24 w-24 sm:h-28 sm:w-28">
                  <s.Illustration />
                </div>
              </div>
              <span className="mb-2 flex h-6 w-6 items-center justify-center rounded-full bg-evergreen-700 text-xs font-semibold text-white">
                {i + 1}
              </span>
              <p className="font-display text-lg text-evergreen-900">{s.title}</p>
              <p className="mt-1.5 max-w-xs text-sm text-ink-600">{s.body}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

// ---------------------------------------------------------------------------
// Stats bar
// ---------------------------------------------------------------------------

import type { PublicStatsDto } from "../../api/types";
import { useFetch } from "../../hooks/useFetch";

function StatsBar() {
  const { t } = useTranslation();
  const { data: stats } = useFetch<PublicStatsDto>("/public/stats");

  const items: { icon: React.ElementType; value: string; label: string }[] = [
    { icon: Users2, value: stats ? `${stats.registeredFarmers.toLocaleString()}+` : "\u2014", label: t("landing.statFarmers") },
    { icon: Building2, value: stats ? `${stats.registeredHotels.toLocaleString()}+` : "\u2014", label: t("landing.statHotels") },
    { icon: Package, value: stats ? `${stats.avgDailyKgDelivered.toLocaleString()} kg` : "\u2014", label: t("landing.statDailyKg") },
    {
      icon: Gauge,
      value: stats?.serviceSatisfactionPercent != null ? `${stats.serviceSatisfactionPercent}%` : t("landing.statSatisfactionPending"),
      label: t("landing.statSatisfaction")
    }
  ];

  return (
    <section id="about" className="border-y border-evergreen-100 bg-white py-10">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <h2 className="mb-6 text-center font-display text-xl text-evergreen-900">{t("landing.statsHeading")}</h2>
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {items.map((s) => (
            <div key={s.label} className="flex items-center gap-3 rounded-lg border border-evergreen-100 bg-evergreen-50/40 p-4">
              <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-evergreen-700 text-white">
                <s.icon className="h-5 w-5" strokeWidth={1.75} />
              </span>
              <div>
                <p className="font-mono text-xl font-semibold text-evergreen-900">{s.value}</p>
                <p className="text-xs text-ink-600">{s.label}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

// ---------------------------------------------------------------------------
// Why AgriLink
// ---------------------------------------------------------------------------

function WhyAgriLink() {
  const { t } = useTranslation();

  const features: { icon: React.ElementType; title: string }[] = [
    { icon: Sprout, title: t("landing.featureDirect") },
    { icon: Truck, title: t("landing.featureDelivery") },
    { icon: ShieldCheck, title: t("landing.featureQuality") },
    { icon: Tag, title: t("landing.featurePrice") }
  ];

  return (
    <section id="why" className="py-14">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <div className="mb-8 text-center">
          <h2 className="font-display text-2xl text-evergreen-900 sm:text-3xl">{t("landing.whyHeading")}</h2>
          <p className="mt-2 text-sm text-ink-600">{t("landing.whySubheading")}</p>
        </div>
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {features.map((f) => (
            <div key={f.title} className="card flex flex-col items-center gap-3 p-5 text-center transition hover:-translate-y-0.5 hover:shadow-md">
              <span className="flex h-12 w-12 items-center justify-center rounded-full bg-wheat-100 text-evergreen-700">
                <f.icon className="h-6 w-6" strokeWidth={1.75} />
              </span>
              <p className="text-sm font-medium text-evergreen-900">{f.title}</p>
            </div>
          ))}
        </div>

        <div id="services" className="mt-14 grid grid-cols-1 gap-6 lg:grid-cols-2">
          <div className="card p-6">
            <h3 className="mb-2 font-display text-lg text-evergreen-900">{t("landing.farmerPortalTitle")}</h3>
            <p className="text-sm text-ink-600">{t("landing.farmerPortalBody")}</p>
          </div>
          <div className="card p-6">
            <h3 className="mb-2 font-display text-lg text-evergreen-900">{t("landing.hotelPortalTitle")}</h3>
            <p className="text-sm text-ink-600">{t("landing.hotelPortalBody")}</p>
          </div>
        </div>
      </div>
    </section>
  );
}

// ---------------------------------------------------------------------------
// Footer
// ---------------------------------------------------------------------------

function SiteFooter() {
  const { t } = useTranslation();

  return (
    <footer id="contact" className="bg-evergreen-950 pt-12 text-evergreen-50">
      <div className="mx-auto grid max-w-7xl grid-cols-1 gap-10 px-4 pb-10 sm:px-6 lg:grid-cols-3">
        <div>
          <div className="flex items-center gap-2">
            <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-evergreen-700 text-white">
              <Sprout className="h-5 w-5" strokeWidth={1.75} />
            </span>
            <span className="font-display text-lg text-white">
              AgriLink <span className="font-sans text-sm font-normal text-wheat-400">Ethiopia</span>
            </span>
          </div>
          <p className="mt-3 max-w-xs text-sm text-evergreen-100/70">{t("landing.footerTagline")}</p>
        </div>

        <div>
          <p className="mb-3 text-xs font-semibold uppercase tracking-widest text-evergreen-100/50">{t("landing.footerLinksHeading")}</p>
          <ul className="space-y-2 text-sm text-evergreen-100/80">
            <li><a href="#home" className="hover:text-white">{t("landing.navHome")}</a></li>
            <li><a href="#about" className="hover:text-white">{t("landing.navAbout")}</a></li>
            <li><a href="#services" className="hover:text-white">{t("landing.navServices")}</a></li>
            <li><a href="#why" className="hover:text-white">{t("landing.footerFaq")}</a></li>
            <li><Link to="/login" className="hover:text-white">{t("landing.footerHelp")}</Link></li>
          </ul>
        </div>

        <div>
          <p className="mb-3 text-xs font-semibold uppercase tracking-widest text-evergreen-100/50">{t("landing.footerContactHeading")}</p>
          <ul className="space-y-2 text-sm text-evergreen-100/80">
            <li className="flex items-center gap-2"><Phone className="h-3.5 w-3.5" /> +251 910 151 570</li>
            <li className="flex items-center gap-2"><Phone className="h-3.5 w-3.5" /> +251 973 872 323</li>
            <li className="flex items-center gap-2"><Mail className="h-3.5 w-3.5" /> agrilink21@gmail.com</li>
            <li className="flex items-center gap-2"><MapPin className="h-3.5 w-3.5" /> {t("landing.footerAddress")}</li>
          </ul>
          <div className="mt-4 flex gap-3">
            {[Facebook, Send, Linkedin, Music2].map((Icon, i) => (
              <a key={i} href="#" className="flex h-8 w-8 items-center justify-center rounded-full bg-evergreen-800 text-evergreen-100 transition hover:bg-wheat-400 hover:text-evergreen-900">
                <Icon className="h-4 w-4" strokeWidth={1.75} />
              </a>
            ))}
          </div>
        </div>
      </div>

      <div className="border-t border-evergreen-800 py-4 text-center text-xs text-evergreen-100/50">
        {t("landing.footerCopyright")}
      </div>
    </footer>
  );
}
