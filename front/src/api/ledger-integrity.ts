import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface LedgerIntegrityCheck {
  id: string;
  walletId: string;
  tenantId: string;
  applicationCode: string | null;
  status: string;
  hashStatus: string;
  issueCount: number;
  severity: string;
  startedAt: string | null;
  completedAt: string | null;
  createdBy: string;
  createdAt: string;
}

export interface LedgerIntegrityCheckDetail extends LedgerIntegrityCheck {
  walletLinks?: { walletId: string; currency: string } | null;
  hashAlgorithm: string;
  hashVersion: string;
  invalidEntries: number;
  issues: LedgerIssue[];
  resultPayload?: Record<string, unknown>;
}

export interface LedgerIssue {
  entryId: string;
  issueType: string;
  description: string;
}

export interface IntegrityActionResult {
  success: boolean;
  checkId?: string;
  message: string;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listLedgerIntegrityChecks(params?: Record<string, unknown>) {
  return get<{ items: LedgerIntegrityCheck[]; total: number }>('/api/v1/admin/reports/financial-security/ledger-integrity', params);
}

export async function getLedgerIntegrityCheck(checkId: string) {
  return get<LedgerIntegrityCheckDetail>(`/api/v1/admin/reports/financial-security/ledger-integrity/${checkId}`);
}

export async function verifyWalletHashChain(walletId: string) {
  return post<IntegrityActionResult>(`/api/v1/admin/diagnostics/ledger/${walletId}/verify-hash-chain`);
}

export async function backfillWalletHashChain(walletId: string) {
  return post<IntegrityActionResult>(`/api/v1/admin/diagnostics/ledger/${walletId}/backfill-hash-chain`);
}

export async function runTenantDeepCheck(req?: { tenantId?: string; applicationCode?: string }) {
  return post<IntegrityActionResult>('/api/v1/admin/diagnostics/ledger/deep-check', req);
}
