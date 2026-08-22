import { apiClient } from '@/api/client';
import type { ApprovalRequestItemDto, ApprovalActionResult, ListApprovalRequestsParams, ListApprovalRequestsResponse } from '../types/approvals.types';

export async function listApprovalRequests(params: ListApprovalRequestsParams, signal?: AbortSignal) {
  const { data } = await apiClient.get<ListApprovalRequestsResponse>('/api/v1/admin/approvals', {
    params,
    signal,
  });
  return data;
}

export async function getApprovalRequest(approvalRequestId: string, signal?: AbortSignal) {
  const { data } = await apiClient.get<ApprovalRequestItemDto>(`/api/v1/admin/approvals/${approvalRequestId}`, { signal });
  return data;
}

export async function approveApprovalRequest(approvalRequestId: string, req?: { note?: string }) {
  const { data } = await apiClient.post<ApprovalActionResult>(`/api/v1/admin/approvals/${approvalRequestId}/approve`, req);
  return data;
}

export async function rejectApprovalRequest(approvalRequestId: string, req: { reason: string }) {
  const { data } = await apiClient.post<ApprovalActionResult>(`/api/v1/admin/approvals/${approvalRequestId}/reject`, req);
  return data;
}

export async function executeApprovalRequest(approvalRequestId: string, req?: { note?: string }) {
  const { data } = await apiClient.post<ApprovalActionResult>(`/api/v1/admin/approvals/${approvalRequestId}/execute`, req);
  return data;
}

export async function cancelApprovalRequest(approvalRequestId: string, req: { reason: string }) {
  const { data } = await apiClient.post<ApprovalActionResult>(`/api/v1/admin/approvals/${approvalRequestId}/cancel`, req);
  return data;
}
