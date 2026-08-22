'use client';

import { createContext, useContext, ReactNode } from 'react';
import { useAuthStore, UserInfo } from '@/stores';

/** Permission check mode */
type PermissionMode = 'exact' | 'anyOf' | 'allOf';

interface PermissionContextValue {
  can: (permission: string | string[], mode?: PermissionMode) => boolean;
  canAll: (permissions: string[]) => boolean;
  canAny: (permissions: string[]) => boolean;
  isAdmin: () => boolean;
  user: UserInfo | null;
  isAuthenticated: boolean;
  permissions: string[];
}

const PermissionContext = createContext<PermissionContextValue | null>(null);

export function PermissionProvider({ children }: { children: ReactNode }) {
  const storeUser = useAuthStore((s) => s.user);
  const storeIsAuthenticated = useAuthStore((s) => s.isAuthenticated);

  // Dev mock auth: when NEXT_PUBLIC_DEV_MOCK_AUTH=true and no real login, provide super_admin
  const isDevMock = typeof window !== 'undefined' && process.env.NEXT_PUBLIC_DEV_MOCK_AUTH === 'true';
  const user = storeUser ?? (isDevMock ? {
    id: 'dev-admin',
    email: 'admin@payment.local',
    firstName: 'Admin',
    lastName: 'User',
    roles: ['admin', 'super_admin'],
    permissions: [] as string[],
  } : null);
  const isAuthenticated = storeIsAuthenticated || isDevMock;
  const permissions = user?.permissions ?? [];

  const can = (permission: string | string[], mode: PermissionMode = 'exact'): boolean => {
    if (!user) return false;
    if (user.roles.includes('super_admin')) return true;
    if (permissions.length === 0) return false;

    if (mode === 'exact') {
      return permissions.includes(permission as string);
    }
    if (mode === 'anyOf') {
      return (permission as string[]).some((p) => permissions.includes(p));
    }
    if (mode === 'allOf') {
      return (permission as string[]).every((p) => permissions.includes(p));
    }
    return false;
  };

  const canAll = (perms: string[]) => can(perms, 'allOf');
  const canAny = (perms: string[]) => can(perms, 'anyOf');
  const isAdmin = () => user?.roles.includes('admin') || user?.roles.includes('super_admin') || false;

  return (
    <PermissionContext.Provider value={{ can, canAll, canAny, isAdmin, user, isAuthenticated, permissions }}>
      {children}
    </PermissionContext.Provider>
  );
}

export function usePermission() {
  const ctx = useContext(PermissionContext);
  if (!ctx) throw new Error('usePermission must be used within PermissionProvider');
  return ctx;
}
