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
      <PhotoGallery />
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
// Hero - a real aerial farmland photograph as the backdrop, a driving-truck
// motif and a few slow-drifting "produce" accents layered on top, matching
// the brand palette already established across the app.
// ---------------------------------------------------------------------------

function Hero() {
  const { t } = useTranslation();

  return (
    <section id="home" className="relative isolate flex min-h-[100svh] items-center overflow-hidden bg-evergreen-950">
      <FieldScene />

      <div className="relative mx-auto w-full max-w-7xl px-4 pb-20 pt-16 sm:px-6 sm:pb-28 sm:pt-24">
        <div className="max-w-2xl">
          <p className="mb-3 inline-block rounded-full bg-white/90 px-3 py-1 text-xs font-semibold uppercase tracking-widest text-evergreen-700 shadow-sm">
            {t("landing.heroEyebrow")}
          </p>
          <h1 className="font-display text-4xl leading-tight text-white drop-shadow-lg sm:text-6xl">
            {t("landing.heroTitleLine1")}
            <br />
            {t("landing.heroTitleLine2")}
          </h1>
          <p className="mt-4 max-w-xl text-base text-white/90 drop-shadow sm:text-lg">
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
    ? { bg: "bg-evergreen-700", ring: "ring-evergreen-300", btn: "bg-evergreen-900 hover:bg-evergreen-800", photo: "https://images.unsplash.com/photo-1624668430039-0175a0fbf006?auto=format&fit=crop&w=600&q=80" }
    : { bg: "bg-[#2B4C6F]", ring: "ring-[#9CB8D4]", btn: "bg-[#1D3A57] hover:bg-[#16304A]", photo: "https://images.unsplash.com/photo-1697611791378-db80b62b0943?auto=format&fit=crop&w=600&q=80" };

  return (
    <Link
      to="/login"
      className={`group relative overflow-hidden rounded-xl ${palette.bg} p-5 text-white shadow-lg ring-1 ${palette.ring} transition hover:-translate-y-0.5 hover:shadow-xl`}
    >
      <img
        src={palette.photo}
        alt=""
        aria-hidden="true"
        className="absolute inset-0 h-full w-full object-cover opacity-25 transition duration-500 group-hover:opacity-35 group-hover:scale-105"
      />
      <div className={`absolute inset-0 bg-gradient-to-t from-black/10 via-transparent to-transparent`} />
      <div className="relative">
        <div className="mb-3 flex h-10 w-10 items-center justify-center rounded-lg bg-white/15">
          <Icon className="h-5 w-5" strokeWidth={1.75} />
        </div>
        <p className="font-display text-lg leading-tight">{title}</p>
        <p className="mt-1 text-xs text-white/75">{subtitle}</p>
        <span className={`mt-4 inline-flex items-center gap-1.5 rounded-md ${palette.btn} px-3 py-1.5 text-xs font-semibold transition`}>
          {cta} <span aria-hidden>&rarr;</span>
        </span>
      </div>
    </Link>
  );
}

/**
 * Real aerial farmland photograph as the hero backdrop, with a soft brand-color
 * gradient laid over it for text contrast, plus the road + driving-truck motif
 * kept along the bottom edge for a touch of motion and personality.
 */
function FieldScene() {
  return (
    <div className="absolute inset-0 -z-10 overflow-hidden bg-evergreen-950">
      {/* Fresh farmland photograph (Unsplash, free licence), served straight from
          the CDN and scaled to cover the whole viewport. */}
      <img
        src="https://images.unsplash.com/photo-1715689565509-117bb045fd14?auto=format&fit=crop&w=2400&q=80"
        srcSet={
          "https://images.unsplash.com/photo-1715689565509-117bb045fd14?auto=format&fit=crop&w=900&q=70 900w," +
          "https://images.unsplash.com/photo-1715689565509-117bb045fd14?auto=format&fit=crop&w=1600&q=75 1600w," +
          "https://images.unsplash.com/photo-1715689565509-117bb045fd14?auto=format&fit=crop&w=2400&q=80 2400w"
        }
        sizes="100vw"
        alt=""
        aria-hidden="true"
        className="absolute inset-0 h-full w-full object-cover object-center"
        loading="eager"
      />
      {/* Dark brand scrim so the heading and body copy stay legible over the photo */}
      <div className="absolute inset-0 bg-gradient-to-r from-evergreen-950/85 via-evergreen-950/55 to-evergreen-950/20" />
      <div className="absolute inset-0 bg-gradient-to-t from-evergreen-950/70 via-transparent to-evergreen-950/30" />

      {/* Drifting ambient "produce" bubbles */}
      <span className="animate-float-slow absolute left-[8%] top-16 h-16 w-16 rounded-full bg-clay-400/25 blur-sm sm:h-24 sm:w-24" />
      <span className="animate-float-slower absolute right-[12%] top-28 h-10 w-10 rounded-full bg-wheat-400/30 blur-sm sm:h-16 sm:w-16" />
      <span className="animate-float-slow absolute left-[38%] top-4 h-8 w-8 rounded-full bg-evergreen-400/20 blur-sm" />
      <span className="animate-float-slower absolute right-[30%] top-10 h-6 w-6 rounded-full bg-clay-300/30 blur-sm" />

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
// Farm to table - real photography for each stage of the journey (harvest
// crate, delivery on the road, hotel destination), sourced from Unsplash.
// ---------------------------------------------------------------------------

function FarmToTable() {
  const { t } = useTranslation();

  const steps: { photo: string; alt: string; title: string; body: string }[] = [
    {
      photo: "https://images.unsplash.com/photo-1624668430039-0175a0fbf006?auto=format&fit=crop&w=400&q=80",
      alt: "Crate of freshly harvested tomatoes",
      title: t("landing.journeyHarvestTitle"),
      body: t("landing.journeyHarvestBody")
    },
    {
      photo: "https://images.unsplash.com/photo-1694113372786-2553caec0c76?auto=format&fit=crop&w=400&q=80",
      alt: "Delivery truck driving down a rural road",
      title: t("landing.journeyDeliveryTitle"),
      body: t("landing.journeyDeliveryBody")
    },
    {
      photo: "https://images.unsplash.com/photo-1697611791378-db80b62b0943?auto=format&fit=crop&w=400&q=80",
      alt: "Hotel building ready to receive fresh produce",
      title: t("landing.journeyServedTitle"),
      body: t("landing.journeyServedBody")
    }
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
              <div className="relative z-10 mb-4 h-32 w-32 overflow-hidden rounded-full bg-white shadow-md ring-4 ring-wheat-50 sm:h-36 sm:w-36">
                <img src={s.photo} alt={s.alt} className="h-full w-full object-cover" loading="lazy" />
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
    <section id="why" className="relative isolate overflow-hidden bg-evergreen-950 py-20">
      {/* Fresh-produce photograph (Unsplash, free licence) behind the whole
          "Why AgriLink" block, dimmed so the cards and copy stay readable. */}
      <img
        src="https://images.unsplash.com/photo-1635341083777-5f93a755e916?auto=format&fit=crop&w=2000&q=80"
        alt=""
        aria-hidden="true"
        loading="lazy"
        className="absolute inset-0 -z-10 h-full w-full object-cover object-center"
      />
      <div className="absolute inset-0 -z-10 bg-evergreen-950/75" />
      <div className="absolute inset-0 -z-10 bg-gradient-to-b from-evergreen-950/60 via-transparent to-evergreen-950/60" />

      <div className="relative mx-auto max-w-7xl px-4 sm:px-6">
        <div className="mb-10 text-center">
          <h2 className="font-display text-2xl text-white drop-shadow-md sm:text-4xl">{t("landing.whyHeading")}</h2>
          <p className="mt-3 text-sm text-white/85 sm:text-base">{t("landing.whySubheading")}</p>
        </div>
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {features.map((f) => (
            <div key={f.title} className="flex flex-col items-center gap-3 rounded-xl border border-white/20 bg-white/10 p-5 text-center text-white shadow-lg backdrop-blur-md transition hover:-translate-y-0.5 hover:bg-white/20 hover:shadow-xl">
              <span className="flex h-12 w-12 items-center justify-center rounded-full bg-wheat-100 text-evergreen-800 shadow">
                <f.icon className="h-6 w-6" strokeWidth={1.75} />
              </span>
              <p className="text-sm font-medium text-white">{f.title}</p>
            </div>
          ))}
        </div>

        <div id="services" className="mt-14 grid grid-cols-1 gap-6 lg:grid-cols-2">
          <div className="card overflow-hidden p-0">
            <img
              src="https://images.unsplash.com/photo-1624668430039-0175a0fbf006?auto=format&fit=crop&w=800&q=80"
              alt="Farmer agent's harvest crate"
              loading="lazy"
              className="h-32 w-full object-cover"
            />
            <div className="p-6">
              <h3 className="mb-2 font-display text-lg text-evergreen-900">{t("landing.farmerPortalTitle")}</h3>
              <p className="text-sm text-ink-600">{t("landing.farmerPortalBody")}</p>
            </div>
          </div>
          <div className="card overflow-hidden p-0">
            <img
              src="https://images.unsplash.com/photo-1697611791378-db80b62b0943?auto=format&fit=crop&w=800&q=80"
              alt="Hotel receiving fresh produce"
              loading="lazy"
              className="h-32 w-full object-cover"
            />
            <div className="p-6">
              <h3 className="mb-2 font-display text-lg text-evergreen-900">{t("landing.hotelPortalTitle")}</h3>
              <p className="text-sm text-ink-600">{t("landing.hotelPortalBody")}</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

// ---------------------------------------------------------------------------
// Photo gallery - a small real-photography strip so the page reads as a
// living marketplace, not just illustrations and numbers.
// ---------------------------------------------------------------------------

function PhotoGallery() {
  const { t } = useTranslation();

  const photos: { src: string; alt: string; span?: string }[] = [
    {
      src: "https://images.unsplash.com/photo-1635774855717-0aec182f92cc?auto=format&fit=crop&w=900&q=80",
      alt: "Basket overflowing with freshly picked vegetables",
      span: "sm:col-span-2 sm:row-span-2"
    },
    {
      src: "https://images.unsplash.com/photo-1635341083777-5f93a755e916?auto=format&fit=crop&w=700&q=80",
      alt: "Wooden crate of assorted fresh produce"
    },
    {
      src: "https://images.unsplash.com/photo-1697611791378-db80b62b0943?auto=format&fit=crop&w=700&q=80",
      alt: "Hotel destination receiving fresh deliveries"
    },
    {
      src: "https://images.unsplash.com/photo-1694113372786-2553caec0c76?auto=format&fit=crop&w=700&q=80",
      alt: "Delivery truck on the road to a hotel"
    }
  ];

  return (
    <section className="bg-white py-14">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <div className="mb-8 text-center">
          <h2 className="font-display text-2xl text-evergreen-900 sm:text-3xl">{t("landing.galleryHeading")}</h2>
          <p className="mt-2 text-sm text-ink-600">{t("landing.gallerySubheading")}</p>
        </div>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-4 sm:gap-4">
          {photos.map((p) => (
            <div key={p.src} className={`group overflow-hidden rounded-xl shadow-sm ring-1 ring-evergreen-100 ${p.span ?? ""}`}>
              <img
                src={p.src}
                alt={p.alt}
                loading="lazy"
                className="h-40 w-full object-cover transition duration-500 group-hover:scale-105 sm:h-full sm:min-h-[160px]"
              />
            </div>
          ))}
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
