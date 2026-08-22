import type { OffsetPage } from '@/shared/api/pagination';

export type AuditHashStatus = 'valid' | 'invalid' | 'unverified' | 'not_applicable';

export interface AuditLogListItemDto {
  id: string;
  actor_type: string;
  actor_user_id: string | null;
  action: string;
  entity_type: string;
  entity_id: string;
  tenant_id: string;
  application_code: string | null;
  request_id: string | null;
  correlation_id: string | null;
  hash_status: AuditHashStatus;
  created_at: string;
}

export interface AuditLogDetailDto extends AuditLogListItemDto {
  ip_address: string | null;
  user_agent: string | null;
  before_snapshot?: Record<string, unknown>;
  after_snapshot?: Record<string, unknown>;
  metadata?: Record<string, unknown>;
  previous_hash: string | null;
  entry_hash: string | null;
  hash_algorithm: string | null;
  hash_version: number | null;
}

export interface ListAuditLogsParams {
  query?: string;
  action?: string;
  dateFrom?: string;
  dateTo?: string;
  skip?: number;
  take?: number;
}

export type ListAuditLogsResponse = OffsetPage<AuditLogListItemDto>;
