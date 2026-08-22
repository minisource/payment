import { apiClient } from '@/api/client';
import type {
  ApprovalPolicyItemDto,
  ApprovalPolicyCreateRequest,
  ApprovalPolicyUpdateRequest,
  ApprovalPolicyActionResult,
  ListApprovalPoliciesParams,
  ListApprovalPoliciesResponse,
} from '../types/approval-policies.types';

export async function listApprovalPolicies(params: ListApprovalPoliciesParams, signal?: AbortSignal) {
  const { data } = await apiClient.get<ListApprovalPoliciesResponse>('/api/v1/admin/approval-policies', {
    params,
    signal,
  });
  return data;
}

export async function getApprovalPolicy(policyId: string, signal?: AbortSignal) {
  const { data } = await apiClient.get<ApprovalPolicyItemDto>(`/api/v1/admin/approval-policies/${policyId}`, { signal });
  return data;
}

export async function createApprovalPolicy(req: ApprovalPolicyCreateRequest) {
  const { data } = await apiClient.post<ApprovalPolicyItemDto>('/api/v1/admin/approval-policies', req);
  return data;
}

export async function updateApprovalPolicy(policyId: string, req: ApprovalPolicyUpdateRequest) {
  const { data } = await apiClient.patch<ApprovalPolicyItemDto>(`/api/v1/admin/approval-policies/${policyId}`, req);
  return data;
}

export async function enableApprovalPolicy(policyId: string) {
  const { data } = await apiClient.post<ApprovalPolicyActionResult>(`/api/v1/admin/approval-policies/${policyId}/enable`);
  return data;
}

export async function disableApprovalPolicy(policyId: string) {
  const { data } = await apiClient.post<ApprovalPolicyActionResult>(`/api/v1/admin/approval-policies/${policyId}/disable`);
  return data;
}

export async function deleteApprovalPolicy(policyId: string) {
  const { data } = await apiClient.delete<ApprovalPolicyActionResult>(`/api/v1/admin/approval-policies/${policyId}`);
  return data;
}
