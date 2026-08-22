import type { ListAuditLogsParams } from '../types/audit.types';

export const auditKeys = {
  all: ['audit'] as const,
  lists: () => [...auditKeys.all, 'list'] as const,
  list: (params: ListAuditLogsParams) => [...auditKeys.lists(), params] as const,
  details: () => [...auditKeys.all, 'detail'] as const,
  detail: (id: string) => [...auditKeys.details(), id] as const,
};
