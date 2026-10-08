import { Languages } from "lucide-react";
import { useTranslation, type Language } from "../i18n/LanguageContext";

const OPTIONS: { value: Language; label: string }[] = [
  { value: "en", label: "English" },
  { value: "am", label: "አማርኛ" }
];

/** Compact EN / AM toggle shown in the sidebar - persists to localStorage via LanguageContext. */
export default function LanguageSwitcher() {
  const { language, setLanguage } = useTranslation();

  return (
    <div className="flex items-center gap-2">
      <Languages className="h-3.5 w-3.5 text-evergreen-100/60" />
      <div className="flex overflow-hidden rounded-md border border-evergreen-700/60">
        {OPTIONS.map((opt) => (
          <button
            key={opt.value}
            onClick={() => setLanguage(opt.value)}
            className={`px-2 py-1 text-xs transition ${
              language === opt.value
                ? "bg-wheat-400 text-evergreen-900 font-medium"
                : "bg-transparent text-evergreen-100/70 hover:text-white"
            }`}
          >
            {opt.label}
          </button>
        ))}
      </div>
    </div>
  );
}
