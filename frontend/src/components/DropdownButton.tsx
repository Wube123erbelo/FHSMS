import { useEffect, useRef, useState, type ReactNode } from "react";
import { ChevronDown } from "lucide-react";

/**
 * One option inside a DropdownButton's menu.
 */
export interface DropdownOption {
  key: string;
  label: string;
  icon?: ReactNode;
  onClick: () => void;
  disabled?: boolean;
}

/**
 * A single trigger button that expands into a menu of choices - the shared
 * building block behind "one button, several options" UI (export formats,
 * online payment providers, etc.) instead of every module rendering its own
 * row of separate buttons. Closes on an outside click, on Escape, and after
 * an option is chosen.
 */
export default function DropdownButton({
  label,
  icon,
  options,
  variant = "secondary",
  align = "left",
  disabled = false
}: {
  label: string;
  icon?: ReactNode;
  options: DropdownOption[];
  variant?: "primary" | "secondary";
  align?: "left" | "right";
  disabled?: boolean;
}) {
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    function onPointerDown(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  const triggerClass = variant === "primary" ? "btn-primary" : "btn-secondary";

  return (
    <div className="relative inline-block" ref={containerRef}>
      <button
        type="button"
        className={triggerClass}
        onClick={() => setOpen((o) => !o)}
        disabled={disabled}
        aria-haspopup="menu"
        aria-expanded={open}
      >
        {icon}
        {label}
        <ChevronDown className={`h-3.5 w-3.5 transition-transform ${open ? "rotate-180" : ""}`} />
      </button>

      {open && (
        <div
          role="menu"
          className={`absolute z-20 mt-1 min-w-[11rem] overflow-hidden rounded-md border border-evergreen-100 bg-white py-1 shadow-card ${
            align === "right" ? "right-0" : "left-0"
          }`}
        >
          {options.map((opt) => (
            <button
              key={opt.key}
              type="button"
              role="menuitem"
              disabled={opt.disabled}
              className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-ink-900 transition hover:bg-evergreen-50 disabled:cursor-not-allowed disabled:opacity-50"
              onClick={() => {
                setOpen(false);
                opt.onClick();
              }}
            >
              {opt.icon}
              {opt.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
