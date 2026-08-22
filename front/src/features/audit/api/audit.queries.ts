'use client';

import { useQuery } from '@tanstack/react-query';
import { auditKeys } from './audit.keys';
import { listAuditLogs, getAuditLog } from './audit.api';
import type { ListAuditLogsParams } from '../types/audit.types';

export function useAuditLogsQuery(params: ListAuditLogsParams) {
  return useQuery({
    queryKey: auditKeys.list(params),
    queryFn: ({ signal }) => listAuditLogs(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function useAuditLogDetailQuery(auditLogId: string | undefined) {
  return useQuery({
    queryKey: auditKeys.detail(auditLogId!),
    queryFn: ({ signal }) => getAuditLog(auditLogId!, signal),
    enabled: !!auditLogId,
  });
}
