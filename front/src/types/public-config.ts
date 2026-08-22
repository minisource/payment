/** Response from GET /v1/system/public-config */
export interface PublicConfig {
  service: string;
  standalone_enabled: boolean;
  auth: AuthConfig;
  tenant: TenantConfig;
  default_tenant_id: string;
  default_application_code: string;
  default_language: string;
  supported_languages: string[];
}

/** Auth section of public config — no secrets exposed */
export interface AuthConfig {
  enabled: boolean;
  mode: 'None' | 'External';
  require_admin_auth: boolean;
  require_api_auth: boolean;
  external_auth_base_url: string;
  external_auth_front_url: string;
}

/** Tenant section of public config */
export interface TenantConfig {
  enabled: boolean;
  mode: 'single' | 'multi';
  show_tenant_switcher: boolean;
  allow_all_tenants_filter: boolean;
}
