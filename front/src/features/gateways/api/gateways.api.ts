import { apiClient } from '@/api/client';
import type { GatewayConfigListItem, GatewayConfigDetail, GatewayProviderDetail, ListGatewayConfigsParams, ListGatewayConfigsResponse, RoutingPolicyListItem } from '../types/gateway.types';

export async function listGatewayConfigs(params?: ListGatewayConfigsParams, signal?: AbortSignal): Promise<ListGatewayConfigsResponse> {
  const { data } = await apiClient.get<ListGatewayConfigsResponse>('/api/v1/admin/gateway-configs', { params, signal });
  return data;
}

export async function getGatewayConfig(gatewayConfigId: string, signal?: AbortSignal): Promise<GatewayConfigDetail> {
  const { data } = await apiClient.get<GatewayConfigDetail>(`/api/v1/admin/gateway-configs/${gatewayConfigId}`, { signal });
  return data;
}

export async function listRoutingPolicies(params?: Record<string, unknown>, signal?: AbortSignal) {
  const { data } = await apiClient.get<{ items: RoutingPolicyListItem[]; total: number }>('/api/v1/admin/gateway-routing-policies', { params, signal });
  return data;
}

export async function getRoutingPolicy(routingPolicyId: string, signal?: AbortSignal) {
  const { data } = await apiClient.get<RoutingPolicyListItem>(`/api/v1/admin/gateway-routing-policies/${routingPolicyId}`, { signal });
  return data;
}

export async function listGatewayProviders(params?: { query?: string; status?: string }, signal?: AbortSignal) {
  const { data } = await apiClient.get<{ items: GatewayProviderDetail[]; total: number }>('/api/v1/admin/gateway-providers', { params, signal });
  return data;
}
