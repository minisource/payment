import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface WalletConsistencyIssue {
  id: string;
  walletId: string;
  tenantId: string;
  applicationCode: string | null;
  issueType: string;
  severity: string;
  status: string;
  expectedBalance: number;
  actualBalance: number;
  currency: string;
  detectedAt: string;
  resolvedAt: string | null;
  createdAt: string;
}

export interface WalletConsistencyIssueDetail extends WalletConsistencyIssue {
  walletLinks?: { walletId: string; currency: string } | null;
  description: string | null;
  resolutionNote: string | null;
  resolvedBy: string | null;
  metadata?: Record<string, unknown>;
}

export interface ConsistencyActionResult {
  success: boolean;
  message: string;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listWalletConsistencyIssues(params?: Record<string, unknown>) {
  return get<{ items: WalletConsistencyIssue[]; total: number }>('/api/v1/admin/wallet-consistency/issues', params);
}

export async function getWalletConsistencyIssue(issueId: string) {
  return get<WalletConsistencyIssueDetail>(`/api/v1/admin/wallet-consistency/issues/${issueId}`);
}

export async function acknowledgeWalletIssue(issueId: string) {
  return post<ConsistencyActionResult>(`/api/v1/admin/wallet-consistency/issues/${issueId}/acknowledge`);
}

export async function resolveWalletIssue(issueId: string, req: { reason: string }) {
  return post<ConsistencyActionResult>(`/api/v1/admin/wallet-consistency/issues/${issueId}/resolve`, req);
}

export async function markWalletIssueFalsePositive(issueId: string, req: { reason: string }) {
  return post<ConsistencyActionResult>(`/api/v1/admin/wallet-consistency/issues/${issueId}/false-positive`, req);
}
