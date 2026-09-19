import { Link } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { useTranslation } from "../i18n/LanguageContext";

/**
 * A small "back to X" link shown above PageHeader. Use this on any page that
 * is reached by drilling into something (a list row, a hub/grid like
 * Settings) rather than being a standalone sidebar destination on its own -
 * without it, a person who arrived via a hub page has no way back to that
 * hub except re-navigating the sidebar from scratch (awkward on mobile,
 * where the sidebar is collapsed behind a menu toggle).
 */
export default function BackLink({ to, label }: { to: string; label?: string }) {
  const { t } = useTranslation();
  return (
    <Link to={to} className="mb-3 inline-flex items-center gap-1 text-sm text-evergreen-700 hover:underline">
      <ArrowLeft className="h-4 w-4" /> {label ?? t("common.back")}
    </Link>
  );
}
