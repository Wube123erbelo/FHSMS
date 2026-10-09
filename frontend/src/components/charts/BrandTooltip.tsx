/**
 * A recharts custom tooltip content renderer, styled to match the app's card
 * language (rounded, soft border, subtle shadow) instead of recharts' plain
 * default box. Used across Dashboard, Agents Management, and Reports so every
 * chart in the app shares one look. Typed loosely (recharts' tooltip payload
 * shape varies by chart type) rather than fighting exact generics here.
 */
export function BrandTooltip({
  active,
  payload,
  label,
  valueFormatter
}: {
  active?: boolean;
  payload?: { name?: string; value?: number | string; color?: string; fill?: string }[];
  label?: string;
  valueFormatter?: (value: number) => string;
}) {
  if (!active || !payload || payload.length === 0) return null;

  return (
    <div className="rounded-lg border border-evergreen-100 bg-white/95 px-3 py-2 text-xs shadow-lg backdrop-blur-sm">
      {label && <p className="mb-1 font-semibold text-evergreen-900">{label}</p>}
      {payload.map((p, i) => (
        <p key={i} className="flex items-center gap-1.5 text-ink-600">
          <span className="inline-block h-2 w-2 rounded-full" style={{ backgroundColor: p.color ?? p.fill }} />
          {p.name && <span>{p.name}:</span>}
          <span className="font-medium text-evergreen-900">
            {typeof p.value === "number" && valueFormatter ? valueFormatter(p.value) : p.value}
          </span>
        </p>
      ))}
    </div>
  );
}
