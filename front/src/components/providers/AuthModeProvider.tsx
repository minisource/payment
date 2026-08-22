'use client';

import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from 'react';
import type { PublicConfig } from '@/types/public-config';
import { fetchPublicConfig } from '@/api/public-config';

interface AuthModeContextValue {
  /** Whether public config has been loaded */
  configLoaded: boolean;
  /** Whether public config failed to load */
  configError: string | null;
  /** The full public config */
  publicConfig: PublicConfig | null;
  /** Auth mode derived from public config */
  authMode: 'None' | 'External' | 'loading';
  /** Whether authentication is required for admin pages */
  isAuthRequired: boolean;
  /** Reload public config */
  reloadConfig: () => Promise<void>;
}

const AuthModeContext = createContext<AuthModeContextValue | null>(null);

const PUBLIC_CONFIG_STORAGE_KEY = 'payment-public-config-cache';

export function AuthModeProvider({ children }: { children: ReactNode }) {
  const [publicConfig, setPublicConfig] = useState<PublicConfig | null>(null);
  const [configLoaded, setConfigLoaded] = useState(false);
  const [configError, setConfigError] = useState<string | null>(null);

  const loadConfig = useCallback(async () => {
    try {
      const config = await fetchPublicConfig();
      setPublicConfig(config);
      setConfigError(null);

      // Cache in localStorage for quick access
      try {
        localStorage.setItem(PUBLIC_CONFIG_STORAGE_KEY, JSON.stringify(config));
      } catch {
        // ignore storage errors
      }
    } catch (err) {
      // Try cache fallback
      try {
        const cached = localStorage.getItem(PUBLIC_CONFIG_STORAGE_KEY);
        if (cached) {
          setPublicConfig(JSON.parse(cached));
          setConfigError('Using cached config — backend unreachable');
        } else {
          setConfigError(err instanceof Error ? err.message : 'Failed to load config');
        }
      } catch {
        setConfigError(err instanceof Error ? err.message : 'Failed to load config');
      }
    } finally {
      setConfigLoaded(true);
    }
  }, []);

  useEffect(() => {
    loadConfig();
  }, [loadConfig]);

  const authMode: AuthModeContextValue['authMode'] = !configLoaded
    ? 'loading'
    : publicConfig?.auth?.mode ?? 'None';

  const isAuthRequired = publicConfig?.auth?.enabled === true && publicConfig?.auth?.require_admin_auth === true;

  return (
    <AuthModeContext.Provider
      value={{
        configLoaded,
        configError,
        publicConfig,
        authMode,
        isAuthRequired,
        reloadConfig: loadConfig,
      }}
    >
      {children}
    </AuthModeContext.Provider>
  );
}

export function useAuthMode(): AuthModeContextValue {
  const ctx = useContext(AuthModeContext);
  if (!ctx) throw new Error('useAuthMode must be used within AuthModeProvider');
  return ctx;
}
