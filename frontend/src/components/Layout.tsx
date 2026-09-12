import { useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import {
  LayoutDashboard, Package, Receipt, ShoppingCart, Warehouse, Percent,
  Truck, Wallet, Landmark, Bell, ScrollText, Users, LogOut, Sprout,
  Menu, X, Sprout as FarmerIcon, ClipboardList, BarChart3, KeyRound,
  Truck as TruckDriverIcon, Settings as SettingsIcon
} from "lucide-react";
import { useAuth } from "../context/AuthContext";
import { useTranslation, type TranslationKey } from "../i18n/LanguageContext";
import { useFetch } from "../hooks/useFetch";
import type { NotificationDto } from "../api/types";
import LanguageSwitcher from "./LanguageSwitcher";

const NAV_SECTIONS: { labelKey: TranslationKey; items: { to: string; labelKey: TranslationKey; icon: React.ElementType; roles?: string[] }[] }[] = [
  {
    labelKey: "nav.overview",
    items: [{ to: "/", labelKey: "nav.dashboard", icon: LayoutDashboard }]
  },
  {
    labelKey: "nav.trade",
    items: [
      { to: "/products", labelKey: "nav.products", icon: Package },
      { to: "/categories", labelKey: "products.category", icon: Package, roles: ["SuperAdmin"] },
      { to: "/units", labelKey: "nav.units", icon: Package, roles: ["SuperAdmin"] },
      { to: "/customers", labelKey: "nav.customers", icon: Users, roles: ["SuperAdmin", "HotelAgent"] },
      { to: "/farmers", labelKey: "nav.farmers", icon: FarmerIcon, roles: ["SuperAdmin", "FarmerAgent"] },
      { to: "/orders", labelKey: "nav.orders", icon: ShoppingCart, roles: ["SuperAdmin", "HotelAgent", "HotelCustomer"] },
      { to: "/invoices", labelKey: "nav.invoices", icon: Receipt, roles: ["SuperAdmin", "HotelAgent"] },
      { to: "/farmer-invoices", labelKey: "nav.farmerInvoices", icon: Receipt, roles: ["SuperAdmin", "FarmerAgent", "Driver"] },
      { to: "/deliveries", labelKey: "nav.deliveries", icon: Truck, roles: ["SuperAdmin"] },
      { to: "/driver-payments", labelKey: "nav.driverPayments", icon: Truck, roles: ["SuperAdmin", "Driver"] },
      { to: "/drivers", labelKey: "nav.adminDrivers", icon: TruckDriverIcon, roles: ["SuperAdmin"] },
      { to: "/driver-portal", labelKey: "nav.driverPortal", icon: TruckDriverIcon, roles: ["Driver"] }
    ]
  },
  {
    labelKey: "nav.operations",
    items: [
      { to: "/inventory", labelKey: "nav.inventory", icon: Warehouse, roles: ["SuperAdmin", "FarmerAgent"] },
      { to: "/work-orders", labelKey: "nav.workOrders", icon: ClipboardList, roles: ["SuperAdmin"] },
      { to: "/commissions", labelKey: "nav.commissions", icon: Wallet, roles: ["SuperAdmin", "HotelAgent", "FarmerAgent"] },
      { to: "/bank-reconciliation", labelKey: "nav.bankReconciliation", icon: Landmark, roles: ["SuperAdmin"] },
      { to: "/notifications", labelKey: "nav.notifications", icon: Bell }
    ]
  },
  {
    labelKey: "nav.admin",
    items: [
      { to: "/agents-management", labelKey: "nav.agentsManagement", icon: BarChart3, roles: ["SuperAdmin"] },
      { to: "/reports", labelKey: "nav.reports", icon: BarChart3, roles: ["SuperAdmin"] },
      { to: "/tax-settings", labelKey: "nav.taxSettings", icon: Percent, roles: ["SuperAdmin"] },
      { to: "/commission-settings", labelKey: "nav.commissionSettings", icon: Percent, roles: ["SuperAdmin"] },
      { to: "/users", labelKey: "nav.users", icon: KeyRound, roles: ["SuperAdmin"] },
      { to: "/audit-logs", labelKey: "nav.auditLogs", icon: ScrollText, roles: ["SuperAdmin"] },
      { to: "/settings", labelKey: "nav.settings", icon: SettingsIcon, roles: ["SuperAdmin"] }
    ]
  }
];

export default function Layout() {
  const { fullName, role, logout, isAuthenticated } = useAuth();
  const { t } = useTranslation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const { data: myNotifications } = useFetch<NotificationDto[]>(isAuthenticated ? "/notifications/mine" : null);
  const unreadCount = myNotifications?.filter((n) => !n.isRead).length ?? 0;

  const sidebarContent = (
    <>
      <div className="flex items-center gap-2 px-5 py-5">
        <Sprout className="h-6 w-6 text-wheat-400" strokeWidth={1.75} />
        <div>
          <p className="font-display text-lg leading-none">AgriLink</p>
          <p className="text-[11px] uppercase tracking-widest text-evergreen-100/60">Ethiopia</p>
        </div>
        <button
          className="ml-auto text-evergreen-100/70 hover:text-white md:hidden"
          onClick={() => setMobileOpen(false)}
          aria-label="Close menu"
        >
          <X className="h-5 w-5" />
        </button>
      </div>

      <nav className="flex-1 overflow-y-auto px-3 py-2">
        {NAV_SECTIONS.map((section) => {
          const visibleItems = section.items.filter((item) => !item.roles || (role && item.roles.includes(role)));
          if (visibleItems.length === 0) return null;
          return (
          <div key={section.labelKey} className="mb-4">
            <p className="px-3 pb-1 pt-3 text-[11px] font-medium uppercase tracking-widest text-evergreen-100/40">
              {t(section.labelKey)}
            </p>
            {visibleItems.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.to === "/"}
                  onClick={() => setMobileOpen(false)}
                  className={({ isActive }) =>
                    `flex items-center gap-2.5 rounded-md px-3 py-2 text-sm transition ${
                      isActive
                        ? "bg-evergreen-700 text-white"
                        : "text-evergreen-100/80 hover:bg-evergreen-700/60 hover:text-white"
                    }`
                  }
                >
                  <item.icon className="h-4 w-4" strokeWidth={1.75} />
                  {t(item.labelKey)}
                  {item.to === "/notifications" && unreadCount > 0 && (
                    <span className="ml-auto flex h-5 min-w-5 items-center justify-center rounded-full bg-clay-500 px-1 text-[10px] font-semibold text-white">
                      {unreadCount > 9 ? "9+" : unreadCount}
                    </span>
                  )}
                </NavLink>
              ))}
          </div>
          );
        })}
      </nav>

      <div className="border-t border-evergreen-700/60 px-4 py-4">
        <div className="mb-3">
          <LanguageSwitcher />
        </div>
        <p className="text-sm font-medium text-white">{fullName}</p>
        <p className="text-xs text-evergreen-100/60">{role}</p>
        <div className="mt-3 flex flex-wrap items-center gap-x-3 gap-y-1">
          <NavLink to="/change-password" className="text-xs text-evergreen-100/70 hover:text-wheat-400">
            {t("auth.changePassword")}
          </NavLink>
          <NavLink to="/two-factor" className="text-xs text-evergreen-100/70 hover:text-wheat-400">
            {t("auth.twoFactorTitle")}
          </NavLink>
          <button onClick={logout} className="flex items-center gap-1 text-xs text-evergreen-100/70 hover:text-wheat-400">
            <LogOut className="h-3.5 w-3.5" /> {t("common.signOut")}
          </button>
        </div>
      </div>
    </>
  );

  return (
    <div className="flex min-h-screen bg-canvas">
      {/* Mobile top bar */}
      <div className="fixed inset-x-0 top-0 z-20 flex items-center justify-between border-b border-evergreen-100 bg-evergreen-900 px-4 py-3 text-white md:hidden">
        <div className="flex items-center gap-2">
          <Sprout className="h-5 w-5 text-wheat-400" />
          <span className="font-display text-base">AgriLink</span>
        </div>
        <button onClick={() => setMobileOpen(true)} aria-label="Open menu">
          <Menu className="h-6 w-6" />
        </button>
      </div>

      {/* Mobile drawer */}
      {mobileOpen && (
        <div className="fixed inset-0 z-30 md:hidden">
          <div className="absolute inset-0 bg-black/40" onClick={() => setMobileOpen(false)} />
          <aside className="absolute inset-y-0 left-0 flex w-72 flex-col bg-evergreen-900 text-evergreen-50 shadow-xl">
            {sidebarContent}
          </aside>
        </div>
      )}

      {/* Desktop sidebar */}
      <aside className="sticky top-0 hidden h-screen w-64 flex-shrink-0 flex-col border-r border-evergreen-100 bg-evergreen-900 text-evergreen-50 md:flex">
        {sidebarContent}
      </aside>

      <main className="flex-1 overflow-y-auto pt-14 md:pt-0">
        <div className="mx-auto max-w-6xl px-4 py-6 sm:px-8 sm:py-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
