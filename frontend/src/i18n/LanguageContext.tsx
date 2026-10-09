import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import en, { type TranslationDictionary } from "./en";
import am from "./am";

export type Language = "en" | "am";

const DICTIONARIES: Record<Language, TranslationDictionary> = { en, am };
const STORAGE_KEY = "fhsms.language";

// TranslationKey was previously a recursive `NestedKeyOf<TranslationDictionary>`
// mapped/conditional type that walked every nested key at compile time for
// autocomplete and typo-catching. The dictionary has grown large and deep
// enough (hundreds of keys, several 3-level-deep namespaces) that TypeScript
// hits its recursive-type instantiation depth limit compiling it
// ("Type instantiation is excessively deep and possibly infinite", TS2589) -
// and once that happens, EVERY t("...") call site fails to type-check, not
// just the ones near the deepest nesting. A plain string is the robust fix:
// it can never blow up regardless of how large the dictionary grows, and
// runtime behavior is unaffected either way - resolve() already falls back
// to returning the raw key when a path isn't found in either language. The
// trade-off is losing compile-time autocomplete/typo-catching on keys.
export type TranslationKey = string;

interface LanguageContextValue {
  language: Language;
  setLanguage: (lang: Language) => void;
  /** Looks up a dot-path key (e.g. "nav.dashboard"), falling back to English, then the key itself. */
  t: (key: TranslationKey) => string;
}

const LanguageContext = createContext<LanguageContextValue | undefined>(undefined);

function resolve(dict: TranslationDictionary, path: string): string | undefined {
  return path.split(".").reduce<unknown>((acc, part) => {
    if (acc && typeof acc === "object" && part in acc) {
      return (acc as Record<string, unknown>)[part];
    }
    return undefined;
  }, dict) as string | undefined;
}

export function LanguageProvider({ children }: { children: ReactNode }) {
  const [language, setLanguageState] = useState<Language>(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored === "am" || stored === "en" ? stored : "en";
  });

  function setLanguage(lang: Language) {
    setLanguageState(lang);
    localStorage.setItem(STORAGE_KEY, lang);
  }

  const t = useMemo(() => {
    return (key: TranslationKey) => {
      const active = resolve(DICTIONARIES[language], key);
      if (active !== undefined) return active;
      const fallback = resolve(DICTIONARIES.en, key);
      return fallback ?? key;
    };
  }, [language]);

  const value = useMemo(() => ({ language, setLanguage, t }), [language, t]);

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>;
}

export function useTranslation() {
  const ctx = useContext(LanguageContext);
  if (!ctx) throw new Error("useTranslation must be used within a LanguageProvider");
  return ctx;
}
