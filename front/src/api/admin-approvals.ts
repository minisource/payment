import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface ApprovalRequest {
  id: string;
  operationType: string;
  tenantId: string;
  applicationCode: string | null;
  status: string;
  requestedByUserId: string;
  subjectType: string;
  subjectId: string;
  requiredApprovals: number;
  approvalsCount: number;
  expiresAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ApprovalRequestDetail extends ApprovalRequest {
  requestPayload?: Record<string, unknown>;
  decisions: ApprovalDecision[];
  riskEvaluationId: string | null;
  executionResult: string | null;
  metadata?: Record<string, unknown>;
}

export interface ApprovalDecision {
  id: string;
  userId: string;
  decision: string;
  note: string | null;
  decidedAt: string;
}

export interface ApprovalActionResult {
  success: boolean;
  message: string;
  executionResult?: Record<string, unknown>;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listApprovalRequests(params?: Record<string, unknown>) {
  return get<{ items: ApprovalRequest[]; total: number }>('/api/v1/admin/approvals', params);
}

export async function getApprovalRequest(approvalRequestId: string) {
  return get<ApprovalRequestDetail>(`/api/v1/admin/approvals/${approvalRequestId}`);
}

export async function approveApprovalRequest(approvalRequestId: string, req?: { note?: string }) {
  return post<ApprovalActionResult>(`/api/v1/admin/approvals/${approvalRequestId}/approve`, req);
}

export async function rejectApprovalRequest(approvalRequestId: string, req: { reason: string }) {
  return post<ApprovalActionResult>(`/api/v1/admin/approvals/${approvalRequestId}/reject`, req);
}

export async function executeApprovalRequest(approvalRequestId: string, req?: { note?: string }) {
  return post<ApprovalActionResult>(`/api/v1/admin/approvals/${approvalRequestId}/execute`, req);
}

export async function cancelApprovalRequest(approvalRequestId: string, req: { reason: string }) {
  return post<ApprovalActionResult>(`/api/v1/admin/approvals/${approvalRequestId}/cancel`, req);
}
