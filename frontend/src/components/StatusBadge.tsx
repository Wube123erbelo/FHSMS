import { useTranslation, type TranslationKey } from "../i18n/LanguageContext";

const PALETTES: Record<string, string> = {
  // greens - good / settled states
  Paid: "bg-evergreen-100 text-evergreen-700",
  Delivered: "bg-evergreen-100 text-evergreen-700",
  Completed: "bg-evergreen-100 text-evergreen-700",
  Matched: "bg-evergreen-100 text-evergreen-700",
  Sent: "bg-evergreen-100 text-evergreen-700",
  Approved: "bg-evergreen-100 text-evergreen-700",
  Confirmed: "bg-evergreen-100 text-evergreen-700",
  // amber - in progress
  Draft: "bg-ink-900/5 text-ink-600",
  Pending: "bg-wheat-100 text-wheat-600",
  Preparing: "bg-wheat-100 text-wheat-600",
  Shipped: "bg-wheat-100 text-wheat-600",
  PartiallyPaid: "bg-wheat-100 text-wheat-600",
  InTransit: "bg-wheat-100 text-wheat-600",
  Issued: "bg-wheat-100 text-wheat-600",
  Unmatched: "bg-wheat-100 text-wheat-600",
  Accrued: "bg-wheat-100 text-wheat-600",
  InProgress: "bg-wheat-100 text-wheat-600",
  // red - problems
  Cancelled: "bg-clay-500/10 text-clay-600",
  Rejected: "bg-clay-500/10 text-clay-600",
  Returned: "bg-clay-500/10 text-clay-600",
  Failed: "bg-clay-500/10 text-clay-600",
  Disputed: "bg-clay-500/10 text-clay-600"
};

export default function StatusBadge({ status }: { status: string }) {
  const { t } = useTranslation();
  const classes = PALETTES[status] ?? "bg-ink-900/5 text-ink-600";
  // t() falls back to returning the key itself when a translation is missing
  // in both languages - if that happens here, show the raw status rather
  // than leaking the "status." key prefix into the UI.
  const key = `status.${status}` as TranslationKey;
  const translated = t(key);
  const label = translated === key ? status : translated;
  return <span className={`badge ${classes}`}>{label}</span>;
}
