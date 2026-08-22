import { get } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface SecurityOverview {
  openRiskCases: number;
  criticalRiskCases: number;
  unresolvedReconciliationItems: number;
  walletsWithConsistencyIssues: number;
  failedLedgerHashChecks: number;
  blockedOperationsToday: number;
  pendingAdminApprovals: number;
  failedSecurityOperations: number;
  latestCriticalCases: LatestCase[];
  latestUnresolvedItems: LatestItem[];
  latestConsistencyIssues: LatestIssue[];
  latestPendingApprovals: LatestApproval[];
}

export interface LatestCase {
  id: string;
  caseType: string;
  severity: string;
  title: string;
  status: string;
  createdAt: string;
}

export interface LatestItem {
  id: string;
  itemType: string;
  status: string;
  severity: string;
  expectedAmount: number;
  actualAmount: number;
  currency: string;
  detectedAt: string;
}

export interface LatestIssue {
  id: string;
  walletId: string;
  issueType: string;
  severity: string;
  status: string;
  detectedAt: string;
}

export interface LatestApproval {
  id: string;
  operationType: string;
  status: string;
  requestedByUserId: string;
  createdAt: string;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function getSecurityOverview() {
  return get<SecurityOverview>('/api/v1/admin/reports/financial-security/overview');
}
