interface PageHeaderProps {
  eyebrow?: string;
  title: string;
  description?: string;
  action?: React.ReactNode;
}

/** Consistent page title block used at the top of every screen. */
export default function PageHeader({ eyebrow, title, description, action }: PageHeaderProps) {
  return (
    <div className="mb-6 flex flex-wrap items-end justify-between gap-4 border-b border-evergreen-100 pb-5">
      <div>
        {eyebrow && (
          <p className="mb-1 font-mono text-xs uppercase tracking-widest text-wheat-600">{eyebrow}</p>
        )}
        <h1 className="text-2xl font-semibold text-evergreen-900">{title}</h1>
        {description && <p className="mt-1 max-w-2xl text-sm text-ink-600">{description}</p>}
      </div>
      {action}
    </div>
  );
}
