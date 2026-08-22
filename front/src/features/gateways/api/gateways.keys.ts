import type { ListGatewayConfigsParams } from '../types/gateway.types';

export const gatewayKeys = {
  all: ['gateways'] as const,
  configs: () => [...gatewayKeys.all, 'configs'] as const,
  configsList: (params: ListGatewayConfigsParams) => [...gatewayKeys.configs(), params] as const,
  configDetail: (id: string) => [...gatewayKeys.configs(), 'detail', id] as const,
  providers: (params?: Record<string, unknown>) =>
    params
      ? [...gatewayKeys.all, 'providers', params] as const
      : [...gatewayKeys.all, 'providers'] as const,
  routingPolicies: () => [...gatewayKeys.all, 'routing'] as const,
  routingPoliciesList: (params: Record<string, unknown>) => [...gatewayKeys.routingPolicies(), params] as const,
  routingPolicyDetail: (id: string) => [...gatewayKeys.routingPolicies(), 'detail', id] as const,
};
