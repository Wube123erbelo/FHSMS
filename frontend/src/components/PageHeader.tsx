interface PageHeaderProps {
  eyebrow?: string;
  title: string;
  description?: string;
  action?: React.ReactNode;
  /** Optional banner photograph shown above the title block, for a more visual page-top. */
  image?: string;
  imageAlt?: string;
}

/** Consistent page title block used at the top of every screen. */
export default function PageHeader({ eyebrow, title, description, action, image, imageAlt }: PageHeaderProps) {
  return (
    <div className="mb-6">
      {image && (
        <div className="relative mb-4 h-24 w-full overflow-hidden rounded-xl shadow-sm sm:h-32">
          <img src={image} alt={imageAlt ?? ""} aria-hidden={!imageAlt} className="h-full w-full object-cover" loading="lazy" />
          <div className="absolute inset-0 bg-gradient-to-t from-evergreen-950/60 via-evergreen-950/5 to-transparent" />
        </div>
      )}
      <div className="flex flex-wrap items-end justify-between gap-4 border-b border-evergreen-100 pb-5">
        <div>
          {eyebrow && (
            <p className="mb-1 font-mono text-xs uppercase tracking-widest text-wheat-600">{eyebrow}</p>
          )}
          <h1 className="text-2xl font-semibold text-evergreen-900">{title}</h1>
          {description && <p className="mt-1 max-w-2xl text-sm text-ink-600">{description}</p>}
        </div>
        {action}
      </div>
    </div>
  );
}
