import type { TenantKeySegment } from '@/features/refunds/api/refunds.keys';

/**
 * Reads tenant selection from localStorage. Used in TanStack Query key generation
 * to include tenant scope in cache keys.
 *
 * Note: This reads localStorage directly (not reactive via React context).
 * It is designed for use inside query hooks where re-renders are driven by
 * parent component state changes that also update localStorage.
 */
export function useTenantSegment(): TenantKeySegment {
  if (typeof window === 'undefined') return { tenantId: null, tenantScope: 'single' };
  const scope = (localStorage.getItem('X-Tenant-Scope') as 'all' | 'single') || 'single';
  const tenantId = scope === 'all' ? null : localStorage.getItem('X-Tenant-Id');
  return { tenantId, tenantScope: scope };
}
