import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export type AuditHashStatus = 'valid' | 'invalid' | 'unverified' | 'not_applicable';

export interface AuditLogListItem {
  id: string;
  actorType: string;
  actorUserId: string | null;
  action: string;
  entityType: string;
  entityId: string;
  tenantId: string;
  applicationCode: string | null;
  requestId: string | null;
  correlationId: string | null;
  hashStatus: AuditHashStatus;
  createdAt: string;
}

export interface AuditLogDetail extends AuditLogListItem {
  ipAddress: string | null;
  userAgent: string | null;
  beforeSnapshot?: Record<string, unknown>;
  afterSnapshot?: Record<string, unknown>;
  metadata?: Record<string, unknown>;
  previousHash: string | null;
  entryHash: string | null;
  hashAlgorithm: string | null;
  hashVersion: number | null;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listAuditLogs(params?: Record<string, unknown>) {
  return get<{ items: AuditLogListItem[]; total: number }>('/api/v1/admin/audit/logs', params);
}

export async function getAuditLog(auditLogId: string) {
  return get<AuditLogDetail>(`/api/v1/admin/audit/logs/${auditLogId}`);
}

export async function verifyAuditHashChain(params?: { auditLogId?: string; tenantId?: string }) {
  return post<{ valid: boolean; message: string }>('/api/v1/admin/audit/verify-hash-chain', params);
}
