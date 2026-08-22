import { get, post, patch, del } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface GatewayProviderDetail {
  code: string;
  displayName: string;
  description: string;
  enabled: boolean;
  adapterType: string;
  supportedCurrencies: string[];
  supportedOperations: string[];
  configSchema: ConfigSchemaField[];
  createdAt: string;
}

export interface ConfigSchemaField {
  key: string;
  label: string;
  type: string;
  required: boolean;
  sensitive: boolean;
  description: string;
  defaultValue?: unknown;
}

export interface GatewayConfigDetail {
  id: string;
  tenantId: string | null;
  applicationCode: string | null;
  providerCode: string;
  name: string;
  description: string | null;
  scope: 'global' | 'tenant' | 'application';
  environment: 'sandbox' | 'production';
  status: string;
  healthStatus: string;
  supportedCurrencies: string[];
  defaultCurrency: string;
  priority: number;
  weight: number;
  minAmount: number | null;
  maxAmount: number | null;
  configValues: Record<string, unknown>;
  secretFieldsStatus: Array<{ key: string; configured: boolean; updatedAt: string | null }>;
  isDefault: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface GatewayConfigListItem {
  id: string;
  tenantId: string | null;
  applicationCode: string | null;
  providerCode: string;
  name: string;
  scope: 'global' | 'tenant' | 'application';
  environment: 'sandbox' | 'production';
  status: string;
  healthStatus: string;
  supportedCurrencies: string[];
  priority: number;
  weight: number;
  minAmount: number | null;
  maxAmount: number | null;
  isDefault: boolean;
  createdAt: string;
}

export interface GatewayConfigCreateRequest {
  name: string;
  description?: string;
  providerCode: string;
  tenantId?: string;
  applicationCode?: string;
  scope: 'global' | 'tenant' | 'application';
  environment: 'sandbox' | 'production';
  status?: string;
  supportedCurrencies: string[];
  defaultCurrency?: string;
  priority?: number;
  weight?: number;
  minAmount?: string;
  maxAmount?: string;
  configValues?: Record<string, string>;
  secretValues?: Record<string, string>;
}

export interface GatewayConfigUpdateRequest extends Partial<GatewayConfigCreateRequest> {
  secretValues?: Record<string, string>;
}

export interface GatewayConfigTestResult {
  success: boolean;
  status: 'healthy' | 'degraded' | 'unhealthy';
  providerCode: string;
  message: string;
  checkedAt: string;
  details?: Record<string, unknown>;
}

export interface RoutingPolicyDetail {
  id: string;
  tenantId: string | null;
  applicationCode: string | null;
  name: string;
  description: string | null;
  scope: 'global' | 'tenant' | 'application';
  strategy: 'priority' | 'random' | 'weighted_random';
  fallbackEnabled: boolean;
  status: string;
  rules: RoutingRule[];
  metadata?: Record<string, unknown>;
  createdAt: string;
  updatedAt: string;
}

export interface RoutingPolicyListItem {
  id: string;
  tenantId: string | null;
  applicationCode: string | null;
  name: string;
  scope: 'global' | 'tenant' | 'application';
  strategy: 'priority' | 'random' | 'weighted_random';
  fallbackEnabled: boolean;
  status: string;
  rulesCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface RoutingRule {
  id?: string;
  providerCode: string;
  gatewayConfigId: string;
  priority: number;
  weight: number;
  currency?: string;
  minAmount?: number;
  maxAmount?: number;
  conditions?: Record<string, unknown>;
  status: string;
}

export interface RoutingPolicyCreateRequest {
  name: string;
  description?: string;
  scope: 'global' | 'tenant' | 'application';
  tenantId?: string;
  applicationCode?: string;
  strategy: 'priority' | 'random' | 'weighted_random';
  fallbackEnabled?: boolean;
  status?: string;
  rules: RoutingRule[];
}

export interface RoutingSimulationRequest {
  amount: string;
  currency: string;
  tenantId?: string;
  applicationCode?: string;
}

export interface RoutingSimulationResponse {
  selectedGatewayConfigId: string | null;
  selectedProviderCode: string | null;
  strategy: string;
  eligibleConfigs: Array<{ gatewayConfigId: string; providerCode: string; reason: string }>;
  rejectedConfigs: Array<{ gatewayConfigId: string; providerCode: string; reason: string }>;
  reason: string;
}

// ═══════════════════════════════════════════════════════════════
// Providers
// ═══════════════════════════════════════════════════════════════

export async function listGatewayProviders(params?: { query?: string; status?: string }) {
  return get<{ items: GatewayProviderDetail[]; total: number }>('/api/v1/admin/gateway-providers', params as Record<string, unknown>);
}

export async function getGatewayProvider(providerCode: string) {
  return get<GatewayProviderDetail>(`/api/v1/admin/gateway-providers/${providerCode}`);
}

// ═══════════════════════════════════════════════════════════════
// Configs
// ═══════════════════════════════════════════════════════════════

export async function listGatewayConfigs(params?: Record<string, unknown>) {
  return get<{ items: GatewayConfigListItem[]; total: number }>('/api/v1/admin/gateway-configs', params);
}

export async function getGatewayConfig(gatewayConfigId: string) {
  return get<GatewayConfigDetail>(`/api/v1/admin/gateway-configs/${gatewayConfigId}`);
}

export async function createGatewayConfig(request: GatewayConfigCreateRequest) {
  return post<GatewayConfigDetail>('/api/v1/admin/gateway-configs', request);
}

export async function updateGatewayConfig(gatewayConfigId: string, request: GatewayConfigUpdateRequest) {
  return patch<GatewayConfigDetail>(`/api/v1/admin/gateway-configs/${gatewayConfigId}`, request);
}

export async function enableGatewayConfig(gatewayConfigId: string) {
  return post<{ success: boolean }>(`/api/v1/admin/gateway-configs/${gatewayConfigId}/enable`);
}

export async function disableGatewayConfig(gatewayConfigId: string, reason?: string) {
  return post<{ success: boolean }>(`/api/v1/admin/gateway-configs/${gatewayConfigId}/disable`, { reason });
}

export async function deleteGatewayConfig(gatewayConfigId: string) {
  return del<{ success: boolean }>(`/api/v1/admin/gateway-configs/${gatewayConfigId}`);
}

export async function testGatewayConfig(gatewayConfigId: string) {
  return post<GatewayConfigTestResult>(`/api/v1/admin/gateway-configs/${gatewayConfigId}/test`);
}

// ═══════════════════════════════════════════════════════════════
// Routing
// ═══════════════════════════════════════════════════════════════

export async function listRoutingPolicies(params?: Record<string, unknown>) {
  return get<{ items: RoutingPolicyListItem[]; total: number }>('/api/v1/admin/gateway-routing-policies', params);
}

export async function getRoutingPolicy(routingPolicyId: string) {
  return get<RoutingPolicyDetail>(`/api/v1/admin/gateway-routing-policies/${routingPolicyId}`);
}

export async function createRoutingPolicy(request: RoutingPolicyCreateRequest) {
  return post<RoutingPolicyDetail>('/api/v1/admin/gateway-routing-policies', request);
}

export async function updateRoutingPolicy(routingPolicyId: string, request: RoutingPolicyCreateRequest) {
  return patch<RoutingPolicyDetail>(`/api/v1/admin/gateway-routing-policies/${routingPolicyId}`, request);
}

export async function deleteRoutingPolicy(routingPolicyId: string) {
  return del<{ success: boolean }>(`/api/v1/admin/gateway-routing-policies/${routingPolicyId}`);
}

export async function simulateRouting(routingPolicyId: string, request: RoutingSimulationRequest) {
  return post<RoutingSimulationResponse>(`/api/v1/admin/gateway-routing-policies/${routingPolicyId}/simulate`, request);
}
