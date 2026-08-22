import { get, post, patch, del } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface ApprovalPolicy {
  id: string;
  name: string;
  description: string | null;
  tenantId: string | null;
  applicationCode: string | null;
  operationType: string;
  status: string;
  amountThreshold: string | null;
  currency: string | null;
  requiredApprovals: number;
  requireDifferentUser: boolean;
  expiresAfterMinutes: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface ApprovalPolicyDetail extends ApprovalPolicy {
  conditions?: Record<string, unknown>;
  metadata?: Record<string, unknown>;
}

export interface ApprovalPolicyCreateRequest {
  name: string;
  description?: string;
  tenantId?: string | null;
  applicationCode?: string | null;
  operationType: string;
  amountThreshold?: string;
  currency?: string;
  requiredApprovals: number;
  requireDifferentUser?: boolean;
  expiresAfterMinutes?: number;
  conditions?: Record<string, unknown>;
}

export interface ApprovalPolicyUpdateRequest extends Partial<ApprovalPolicyCreateRequest> {
  status?: string;
}

export interface ApprovalPolicyActionResult {
  success: boolean;
  message: string;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listApprovalPolicies(params?: Record<string, unknown>) {
  return get<{ items: ApprovalPolicy[]; total: number }>('/api/v1/admin/approval-policies', params);
}

export async function getApprovalPolicy(policyId: string) {
  return get<ApprovalPolicyDetail>(`/api/v1/admin/approval-policies/${policyId}`);
}

export async function createApprovalPolicy(req: ApprovalPolicyCreateRequest) {
  return post<ApprovalPolicy>(`/api/v1/admin/approval-policies`, req);
}

export async function updateApprovalPolicy(policyId: string, req: ApprovalPolicyUpdateRequest) {
  return patch<ApprovalPolicy>(`/api/v1/admin/approval-policies/${policyId}`, req);
}

export async function enableApprovalPolicy(policyId: string) {
  return post<ApprovalPolicyActionResult>(`/api/v1/admin/approval-policies/${policyId}/enable`);
}

export async function disableApprovalPolicy(policyId: string) {
  return post<ApprovalPolicyActionResult>(`/api/v1/admin/approval-policies/${policyId}/disable`);
}

export async function deleteApprovalPolicy(policyId: string) {
  return del<ApprovalPolicyActionResult>(`/api/v1/admin/approval-policies/${policyId}`);
}
