import type { TenantScope } from '@/hooks/use-tenant';

export interface TenantKeySegment {
  tenantId: string | null;
  tenantScope: TenantScope;
}

export const refundKeys = {
  all: ({ tenantId, tenantScope }: TenantKeySegment) =>
    ['refunds', tenantScope, tenantId ?? 'all'] as const,
  lists: (tenant: TenantKeySegment) =>
    [...refundKeys.all(tenant), 'list'] as const,
  list: (tenant: TenantKeySegment, params: Record<string, unknown>) =>
    [...refundKeys.lists(tenant), params] as const,
  details: (tenant: TenantKeySegment) =>
    [...refundKeys.all(tenant), 'detail'] as const,
  detail: (tenant: TenantKeySegment, id: string) =>
    [...refundKeys.details(tenant), id] as const,
};
