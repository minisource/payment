import { apiClient } from '@/api/client';
import type { TenantListResponse } from '../types/tenant.types';

export async function listTenants(signal?: AbortSignal): Promise<TenantListResponse> {
  const { data } = await apiClient.get<TenantListResponse>('/api/v1/admin/tenants', { signal });
  return data;
}
