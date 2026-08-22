import type { OffsetPage } from '@/shared/api/pagination';

export type GatewayScope = 'global' | 'tenant' | 'application';
export type GatewayEnvironment = 'sandbox' | 'production';
export type GatewayHealthStatus = 'healthy' | 'degraded' | 'unhealthy';

export interface GatewayConfigListItem {
  id: string;
  tenant_id: string | null;
  application_code: string | null;
  provider_code: string;
  name: string;
  scope: GatewayScope;
  environment: GatewayEnvironment;
  status: string;
  health_status: GatewayHealthStatus;
  supported_currencies: string[];
  priority: number;
  weight: number;
  min_amount: number | null;
  max_amount: number | null;
  is_default: boolean;
  created_at: string;
}

export interface GatewayConfigDetail {
  id: string;
  tenant_id: string | null;
  application_code: string | null;
  provider_code: string;
  name: string;
  description: string | null;
  scope: GatewayScope;
  environment: GatewayEnvironment;
  status: string;
  health_status: GatewayHealthStatus;
  supported_currencies: string[];
  default_currency: string;
  priority: number;
  weight: number;
  min_amount: number | null;
  max_amount: number | null;
  config_values: Record<string, unknown>;
  secret_fields_status: Array<{ key: string; configured: boolean; updated_at: string | null }>;
  is_default: boolean;
  created_at: string;
  updated_at: string;
}

export interface GatewayProviderDetail {
  code: string;
  display_name: string;
  description: string;
  enabled: boolean;
  adapter_type: string;
  supported_currencies: string[];
  supported_operations: string[];
  config_schema: ConfigSchemaField[];
  created_at: string;
}

export interface ConfigSchemaField {
  key: string;
  label: string;
  type: string;
  required: boolean;
  sensitive: boolean;
  description: string;
}

export interface ListGatewayConfigsParams {
  query?: string;
  status?: string;
  environment?: string;
  provider_code?: string;
  skip?: number;
  take?: number;
}

export interface RoutingPolicyListItem {
  id: string;
  tenant_id: string | null;
  application_code: string | null;
  name: string;
  description: string | null;
  scope: 'global' | 'tenant' | 'application';
  strategy: 'priority' | 'random' | 'weighted_random';
  fallback_enabled: boolean;
  status: string;
  rules_count: number;
  metadata?: Record<string, unknown>;
  created_at: string;
  updated_at: string;
}

export interface ListRoutingPoliciesParams {
  query?: string;
  strategy?: string;
  status?: string;
  scope?: string;
  skip?: number;
  take?: number;
}

export type ListRoutingPoliciesResponse = OffsetPage<RoutingPolicyListItem>;

export type ListGatewayConfigsResponse = OffsetPage<GatewayConfigListItem>;
