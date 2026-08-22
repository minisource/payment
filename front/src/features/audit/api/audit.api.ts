import { apiClient } from '@/api/client';
import type { ListAuditLogsParams, ListAuditLogsResponse, AuditLogDetailDto } from '../types/audit.types';

export async function listAuditLogs(params?: ListAuditLogsParams, signal?: AbortSignal): Promise<ListAuditLogsResponse> {
  const { data } = await apiClient.get<ListAuditLogsResponse>('/api/v1/admin/audit/logs', { params, signal });
  return data;
}

export async function getAuditLog(auditLogId: string, signal?: AbortSignal): Promise<AuditLogDetailDto> {
  const { data } = await apiClient.get<AuditLogDetailDto>(`/api/v1/admin/audit/logs/${auditLogId}`, { signal });
  return data;
}
