'use client';

import { ReactNode } from 'react';
import { usePermission } from '@/hooks/use-permission';

interface PermissionGuardProps {
  permission: string | string[];
  mode?: 'exact' | 'anyOf' | 'allOf';
  children: ReactNode;
  fallback?: ReactNode;
}

/** Hides children if user lacks the required permission(s) */
export function PermissionGuard({ permission, mode = 'exact', children, fallback = null }: PermissionGuardProps) {
  const { can } = usePermission();
  if (!can(permission, mode)) return <>{fallback}</>;
  return <>{children}</>;
}

interface RoutePermissionGuardProps {
  permissions: string[];
  children: ReactNode;
}

/** Renders children only if user has ALL listed permissions (for route-level guarding) */
export function RoutePermissionGuard({ permissions, children }: RoutePermissionGuardProps) {
  const { canAll, isAuthenticated, user } = usePermission();

  if (!isAuthenticated || !user) {
    return (
      <div className="flex min-h-[300px] items-center justify-center">
        <p className="text-muted-foreground">Please log in to access this page.</p>
      </div>
    );
  }

  if (!canAll(permissions)) {
    return (
      <div className="flex min-h-[300px] flex-col items-center justify-center space-y-2">
        <p className="text-lg font-medium text-destructive">Access Denied</p>
        <p className="text-sm text-muted-foreground">You do not have the required permissions to view this page.</p>
      </div>
    );
  }

  return <>{children}</>;
}

/** Hides children if user lacks ANY of the listed permissions (action-level) */
export function ActionPermissionGuard({ permission, mode = 'exact', children, fallback = null }: PermissionGuardProps) {
  return <PermissionGuard permission={permission} mode={mode} fallback={fallback}>{children}</PermissionGuard>;
}
