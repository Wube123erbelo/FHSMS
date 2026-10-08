import { Link } from "react-router-dom";
import {
  Percent, Wallet, KeyRound, ShieldCheck, ScrollText, Bell
} from "lucide-react";
import PageHeader from "../../components/PageHeader";
import { useTranslation } from "../../i18n/LanguageContext";

/**
 * A single entry point for every admin-configurable part of the system.
 * Each of these already exists as its own full page (Tax, Commissions,
 * Users, 2FA, Audit Logs, Notifications) - this just gathers them behind
 * one "Settings" link instead of leaving an admin to hunt through the
 * sidebar's Trade/Operations/Admin groups to find them.
 */
export default function SettingsPage() {
  const { t } = useTranslation();

  const groups: {
    title: string;
    items: { to: string; icon: React.ElementType; label: string; description: string }[];
  }[] = [
    {
      title: t("settings.groupMoney"),
      items: [
        { to: "/tax-settings", icon: Percent, label: t("nav.taxSettings"), description: t("settings.taxDescription") },
        { to: "/commission-settings", icon: Percent, label: t("nav.commissionSettings"), description: t("settings.commissionSettingsDescription") },
        { to: "/commissions", icon: Wallet, label: t("settings.commissionRules"), description: t("settings.commissionDescription") }
      ]
    },
    {
      title: t("settings.groupAccess"),
      items: [
        { to: "/users", icon: KeyRound, label: t("nav.users"), description: t("settings.usersDescription") },
        { to: "/two-factor", icon: ShieldCheck, label: t("settings.twoFactor"), description: t("settings.twoFactorDescription") },
        { to: "/audit-logs", icon: ScrollText, label: t("nav.auditLogs"), description: t("settings.auditDescription") }
      ]
    },
    {
      title: t("settings.groupGeneral"),
      items: [
        { to: "/notifications", icon: Bell, label: t("nav.notifications"), description: t("settings.notificationsDescription") }
      ]
    }
  ];

  return (
    <div>
      <PageHeader title={t("settings.title")} description={t("settings.description")} />

      {groups.map((group) => (
        <div key={group.title} className="mb-8">
          <h2 className="mb-3 text-xs font-semibold uppercase tracking-widest text-wheat-600">{group.title}</h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {group.items.map((item) => (
              <Link
                key={item.to}
                to={item.to}
                className="card flex items-start gap-3 p-4 transition hover:shadow-md hover:border-evergreen-200"
              >
                <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-evergreen-50 text-evergreen-700">
                  <item.icon className="h-5 w-5" strokeWidth={1.75} />
                </span>
                <div className="min-w-0">
                  <p className="text-sm font-semibold text-evergreen-900">{item.label}</p>
                  <p className="mt-0.5 text-xs text-ink-600">{item.description}</p>
                </div>
              </Link>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
