'use client';

import { createContext, useContext, useState, useCallback, ReactNode } from 'react';
import { resolveLanguage, setLanguage } from './language';

type Lang = 'fa' | 'en';

interface LanguageContextValue {
  lang: Lang;
  toggleLanguage: () => void;
  setLang: (lang: Lang) => void;
}

const LanguageContext = createContext<LanguageContextValue>({
  lang: 'fa',
  toggleLanguage: () => {},
  setLang: () => {},
});

/**
 * Provides reactive language state to the entire app.
 * Initializes from localStorage, updates both state + localStorage on change.
 */
export function LanguageProvider({ children }: { children: ReactNode }) {
  const [lang, setLangState] = useState<Lang>(() => resolveLanguage());

  const setLang = useCallback((newLang: Lang) => {
    setLangState(newLang);
    setLanguage(newLang);
  }, []);

  const toggleLanguage = useCallback(() => {
    setLangState(prev => {
      const next = prev === 'fa' ? 'en' : 'fa';
      setLanguage(next);
      return next;
    });
  }, []);

  return (
    <LanguageContext.Provider value={{ lang, toggleLanguage, setLang }}>
      {children}
    </LanguageContext.Provider>
  );
}

/**
 * Returns the full language context: { lang, toggleLanguage, setLang }.
 * Falls back to defaults if used outside LanguageProvider.
 */
export function useLang() {
  return useContext(LanguageContext);
}

/**
 * Returns a reactive translate function bound to the current context language.
 * Usage: const { t } = useT();  t('error.somethingWentWrong')
 */
export function useT() {
  const { lang } = useLang();
  const t = useCallback(
    (key: string) => translateKey(key, lang),
    [lang],
  );
  return { t, lang };
}



// ─── Internal ────────────────────────────────────────

import { translations as translationMap } from './translations';

function translateKey(key: string, lang: Lang): string {
  const entry = (translationMap as Record<string, Record<Lang, string>>)[key];
  if (!entry) return key;
  return entry[lang] ?? entry['en'] ?? key;
}
