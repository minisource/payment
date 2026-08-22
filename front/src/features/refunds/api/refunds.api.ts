import { apiClient } from '@/api/client';
import type {
  RefundRequestDto,
  RefundListResponse,
  CreateRefundRequest,
  ApproveRefundRequest,
  RejectRefundRequest,
  ListRefundsParams,
} from '../types/refund.types';

export async function listRefunds(
  params?: ListRefundsParams,
  signal?: AbortSignal,
): Promise<RefundListResponse> {
  const { data } = await apiClient.get<RefundListResponse>('/api/v1/admin/refunds', { params, signal });
  return data;
}

export async function getRefund(
  refundId: string,
  signal?: AbortSignal,
): Promise<RefundRequestDto> {
  const { data } = await apiClient.get<RefundRequestDto>(`/api/v1/admin/refunds/${refundId}`, { signal });
  return data;
}

export async function createRefund(
  request: CreateRefundRequest,
): Promise<RefundRequestDto> {
  const { data } = await apiClient.post<RefundRequestDto>('/api/v1/admin/refunds', request);
  return data;
}

export async function approveRefund(
  refundId: string,
  request?: ApproveRefundRequest,
): Promise<RefundRequestDto> {
  const { data } = await apiClient.post<RefundRequestDto>(
    `/api/v1/admin/refunds/${refundId}/approve`,
    request ?? {},
  );
  return data;
}

export async function rejectRefund(
  refundId: string,
  request: RejectRefundRequest,
): Promise<RefundRequestDto> {
  const { data } = await apiClient.post<RefundRequestDto>(
    `/api/v1/admin/refunds/${refundId}/reject`,
    request,
  );
  return data;
}

export async function cancelRefund(refundId: string): Promise<RefundRequestDto> {
  const { data } = await apiClient.post<RefundRequestDto>(
    `/api/v1/admin/refunds/${refundId}/cancel`,
  );
  return data;
}

export async function processRefund(refundId: string): Promise<RefundRequestDto> {
  const { data } = await apiClient.post<RefundRequestDto>(
    `/api/v1/admin/refunds/${refundId}/process`,
  );
  return data;
}

export async function retryRefund(refundId: string): Promise<RefundRequestDto> {
  const { data } = await apiClient.post<RefundRequestDto>(
    `/api/v1/admin/refunds/${refundId}/retry`,
  );
  return data;
}

export async function markManualReview(
  refundId: string,
  reason: string,
): Promise<RefundRequestDto> {
  const { data } = await apiClient.post<RefundRequestDto>(
    `/api/v1/admin/refunds/${refundId}/mark-manual-review`,
    JSON.stringify(reason),
    { headers: { 'Content-Type': 'application/json' } },
  );
  return data;
}
