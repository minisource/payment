import { useQuery } from '@tanstack/react-query';
import { tenantKeys } from './tenants.keys';
import * as api from './tenants.api';

export function useTenantsQuery(enabled: boolean) {
  return useQuery({
    queryKey: tenantKeys.list(),
    queryFn: ({ signal }) => api.listTenants(signal),
    enabled,
    staleTime: 5 * 60 * 1000, // 5 min cache
  });
}
