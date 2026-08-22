/**
 * Language resolver utility for the Payment Admin Frontend.
 *
 * Detects the user's preferred language and normalizes it to "fa" or "en".
 * The resolved language is sent to the backend via X-Language and Accept-Language headers
 * so that error messages are returned in the appropriate language.
 */

type SupportedLanguage = 'fa' | 'en';

const SUPPORTED_LANGUAGES: SupportedLanguage[] = ['fa', 'en'];

/**
 * Normalizes a raw language string to "fa" or "en".
 *
 * Normalization rules:
 * - fa, fa-IR, fa_IR, fa-ir → fa
 * - en, en-US, en_US, en-us → en
 * - Unsupported → attempts to detect the primary language tag; unsupported → fallback to "fa"
 */
export function normalizeLanguage(raw: string): SupportedLanguage {
  if (!raw) return 'fa';

  const cleaned = raw.trim().toLowerCase().replace(/_/g, '-');
  const primaryTag = cleaned.split('-')[0];

  if (primaryTag === 'fa') return 'fa';
  if (primaryTag === 'en') return 'en';

  // Unsupported primary language → fallback to fa
  return 'fa';
}

/**
 * Resolves the current user language using this priority:
 * 1. Previously saved preference in localStorage
 * 2. Browser language (navigator.language)
 * 3. Fallback to "fa"
 */
export function resolveLanguage(): SupportedLanguage {
  // Priority 1: saved preference (client-side only)
  if (typeof window !== 'undefined') {
    const saved = localStorage.getItem('X-Language');
    if (saved) return normalizeLanguage(saved);

    // Priority 2: browser language
    if (navigator.language) {
      return normalizeLanguage(navigator.language);
    }
  }

  // Fallback (SSR-safe)
  return 'fa';
}

/**
 * Sets the language preference in localStorage for persistence.
 */
export function setLanguage(lang: SupportedLanguage): void {
  if (typeof window !== 'undefined') {
    localStorage.setItem('X-Language', lang);
  }
}

/**
 * Returns the Accept-Language header value for the given language.
 */
export function getAcceptLanguageHeader(lang: SupportedLanguage): string {
  if (lang === 'fa') return 'fa-IR,fa;q=0.9,en;q=0.8';
  return 'en-US,en;q=0.9,fa;q=0.8';
}

/**
 * Returns the X-Language header value for the given language.
 */
export function getLanguageHeader(lang: SupportedLanguage): string {
  return lang;
}
