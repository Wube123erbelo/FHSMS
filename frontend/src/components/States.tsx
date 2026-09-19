import { AlertTriangle, Inbox, Loader2 } from "lucide-react";

export function LoadingState({ label = "Loading" }: { label?: string }) {
  return (
    <div className="flex items-center gap-2 py-10 text-sm text-ink-600">
      <Loader2 className="h-4 w-4 animate-spin" /> {label}...
    </div>
  );
}

export function ErrorState({ message }: { message: string }) {
  return (
    <div className="flex items-start gap-2 rounded-md border border-clay-500/30 bg-clay-500/5 px-4 py-3 text-sm text-clay-600">
      <AlertTriangle className="mt-0.5 h-4 w-4 flex-shrink-0" /> {message}
    </div>
  );
}

export function EmptyState({ title, description }: { title: string; description?: string }) {
  return (
    <div className="flex flex-col items-center gap-2 rounded-lg border border-dashed border-evergreen-200 py-14 text-center">
      <Inbox className="h-6 w-6 text-evergreen-300" />
      <p className="text-sm font-medium text-ink-900">{title}</p>
      {description && <p className="max-w-sm text-xs text-ink-300">{description}</p>}
    </div>
  );
}
