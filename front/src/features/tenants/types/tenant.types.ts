export interface TenantDto {
  id: string;
  name: string;
  code: string;
  is_default: boolean;
}

export interface TenantListResponse {
  items: TenantDto[];
  can_view_all: boolean;
}

export type TenantScope = 'all' | 'single';
