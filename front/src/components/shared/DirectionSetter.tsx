'use client';

import { useEffect } from 'react';
import { useLang } from '@/shared/i18n/LanguageProvider';

/**
 * Sets the `dir` and `lang` attributes on the <html> element based on the current language.
 * Renders nothing visually — operates entirely via DOM side-effects.
 *
 * Must be mounted inside a LanguageProvider (via Providers) to be reactive.
 */
export function DirectionSetter() {
  const { lang } = useLang();

  useEffect(() => {
    const html = document.documentElement;
    html.dir = lang === 'fa' ? 'rtl' : 'ltr';
    html.lang = lang;
  }, [lang]);

  return null;
}
