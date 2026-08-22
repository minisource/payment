'use client';

import { createContext, useContext, useState, useCallback, useEffect, ReactNode } from 'react';
import type { TenantDto } from '@/features/tenants/types/tenant.types';
import { useTenantsQuery } from '@/features/tenants/api/tenants.queries';

export type TenantScope = 'all' | 'single';

export interface TenantSelection {
  scope: TenantScope;
  tenantId: string | null;
  tenantName?: string | null;
}

interface TenantContextValue {
  tenantId: string | null;
  setTenantId: (id: string | null) => void;
  tenantName: string | null;
  setTenantName: (name: string | null) => void;
  tenants: TenantDto[];
  selectedTenant: TenantSelection;
  canViewAllTenants: boolean;
  showTenantSwitcher: boolean;
  setSelectedTenant: (selection: TenantSelection) => void;
  isLoadingTenants: boolean;
}

const TENANT_STORAGE_KEY = 'X-Tenant-Id';
const TENANT_SCOPE_KEY = 'X-Tenant-Scope';

const TenantContext = createContext<TenantContextValue | null>(null);

export function TenantProvider({ children }: { children: ReactNode }) {
  const [tenantId, setTenantId] = useState<string | null>(() => {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(TENANT_STORAGE_KEY);
    }
    return null;
  });
  const [tenantName, setTenantName] = useState<string | null>(null);
  const [selectedTenant, setSelectedTenantState] = useState<TenantSelection>(() => {
    if (typeof window !== 'undefined') {
      const storedScope = localStorage.getItem(TENANT_SCOPE_KEY) as TenantScope | null;
      const storedId = localStorage.getItem(TENANT_STORAGE_KEY);
      if (storedScope === 'all') return { scope: 'all', tenantId: null };
      if (storedId) return { scope: 'single', tenantId: storedId };
    }
    return { scope: 'single', tenantId: null };
  });

  // Always fetch available tenants (5-min staleTime). UI visibility is derived from list length.
  const { data: tenantData, isLoading: isLoadingTenants } = useTenantsQuery(true);
  const tenants = tenantData?.items ?? [];
  const canViewAllTenants = tenantData?.can_view_all ?? false;

  // Show tenant switcher only when multiple tenants are available
  const showTenantSwitcher = tenants.length > 1;

  // Sync selected tenant with localStorage
  useEffect(() => {
    if (typeof window !== 'undefined') {
      if (selectedTenant.scope === 'all') {
        localStorage.setItem(TENANT_SCOPE_KEY, 'all');
        localStorage.removeItem(TENANT_STORAGE_KEY);
      } else if (selectedTenant.tenantId) {
        localStorage.setItem(TENANT_SCOPE_KEY, 'single');
        localStorage.setItem(TENANT_STORAGE_KEY, selectedTenant.tenantId);
      }
    }
  }, [selectedTenant]);

  // If tenant list loaded and no tenant selected, auto-select first/default
  useEffect(() => {
    if (!isLoadingTenants && tenants.length > 0 && !tenantId && selectedTenant.scope !== 'all') {
      const defaultTenant = tenants.find(t => t.is_default) ?? tenants[0];
      setTenantId(defaultTenant.id);
      setTenantName(defaultTenant.name);
      setSelectedTenantState({ scope: 'single', tenantId: defaultTenant.id });
    }
  }, [isLoadingTenants, tenants, tenantId, selectedTenant.scope]);

  const setSelectedTenant = useCallback((selection: TenantSelection) => {
    setSelectedTenantState(selection);
    if (selection.scope === 'all') {
      setTenantId(null);
      setTenantName('All Tenants');
    } else if (selection.tenantId) {
      setTenantId(selection.tenantId);
      const tenant = tenants.find(t => t.id === selection.tenantId);
      setTenantName(tenant?.name ?? selection.tenantName ?? null);
    }
  }, [tenants]);

  const handleSetTenantId = useCallback((id: string | null) => {
    setTenantId(id);
    if (typeof window !== 'undefined') {
      if (id) {
        localStorage.setItem(TENANT_STORAGE_KEY, id);
        localStorage.setItem(TENANT_SCOPE_KEY, 'single');
      } else {
        localStorage.removeItem(TENANT_STORAGE_KEY);
        localStorage.setItem(TENANT_SCOPE_KEY, 'all');
      }
    }
  }, []);

  return (
    <TenantContext.Provider value={{
      tenantId, setTenantId: handleSetTenantId,
      tenantName, setTenantName: useCallback((n) => setTenantName(n), []),
      tenants, selectedTenant, canViewAllTenants, showTenantSwitcher,
      setSelectedTenant, isLoadingTenants,
    }}>
      {children}
    </TenantContext.Provider>
  );
}

export function useTenant() {
  const ctx = useContext(TenantContext);
  if (!ctx) throw new Error('useTenant must be used within TenantProvider');
  return ctx;
}

// ─── Application Code Provider ───────────────────────────

interface ApplicationContextValue {
  applicationCode: string;
  setApplicationCode: (code: string) => void;
}

const ApplicationContext = createContext<ApplicationContextValue>({
  applicationCode: 'payment-admin',
  setApplicationCode: () => {},
});

export function ApplicationProvider({ children }: { children: ReactNode }) {
  const [applicationCode, setApplicationCode] = useState('payment-admin');

  return (
    <ApplicationContext.Provider value={{ applicationCode, setApplicationCode }}>
      {children}
    </ApplicationContext.Provider>
  );
}

export function useApplication() {
  return useContext(ApplicationContext);
}
