import { useQuery } from '@tanstack/react-query';
import { gatewayKeys } from './gateways.keys';
import { listGatewayConfigs, getGatewayConfig, listGatewayProviders, listRoutingPolicies, getRoutingPolicy } from './gateways.api';
import type { ListGatewayConfigsParams } from '../types/gateway.types';

export function useGatewayConfigsQuery(params: ListGatewayConfigsParams) {
  return useQuery({
    queryKey: gatewayKeys.configsList(params),
    queryFn: ({ signal }) => listGatewayConfigs(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function useGatewayConfigDetailQuery(gatewayConfigId: string | undefined) {
  return useQuery({
    queryKey: gatewayKeys.configDetail(gatewayConfigId ?? ''),
    queryFn: ({ signal }) => getGatewayConfig(gatewayConfigId!, signal),
    enabled: !!gatewayConfigId,
  });
}

export function useRoutingPoliciesQuery(params: Record<string, unknown>) {
  return useQuery({
    queryKey: gatewayKeys.routingPoliciesList(params),
    queryFn: ({ signal }) => listRoutingPolicies(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function useRoutingPolicyDetailQuery(routingPolicyId: string | undefined) {
  return useQuery({
    queryKey: gatewayKeys.routingPolicyDetail(routingPolicyId ?? ''),
    queryFn: ({ signal }) => getRoutingPolicy(routingPolicyId!, signal),
    enabled: !!routingPolicyId,
  });
}

export function useGatewayProvidersQuery(params?: { query?: string; status?: string }) {
  return useQuery({
    queryKey: gatewayKeys.providers(params as Record<string, unknown>),
    queryFn: ({ signal }) => listGatewayProviders(params, signal),
    placeholderData: (prev) => prev,
  });
}
