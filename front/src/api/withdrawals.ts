import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface WithdrawalItem {
  id: string;
  tenantId: string;
  ownerType: string;
  ownerId: string;
  walletId: string;
  payoutAccountId: string;
  accountType: string;
  cardMasked: string | null;
  ibanMasked: string | null;
  amount: number;
  feeAmount: number;
  netAmount: number;
  currency: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface WithdrawalDetail extends WithdrawalItem {
  applicationCode: string | null;
  holderName: string;
  bankName: string | null;
  requestedAt: string;
  approvedAt: string | null;
  approvedByUserId: string | null;
  rejectedAt: string | null;
  rejectionReason: string | null;
  cancelledAt: string | null;
  processingStartedAt: string | null;
  paidAt: string | null;
  failedAt: string | null;
  failureReason: string | null;
  adminNote: string | null;
  lockTransactionId: string | null;
  releaseTransactionId: string | null;
  captureTransactionId: string | null;
  walletLinks: {
    walletId: string;
    availableBalance: number;
    lockedBalance: number;
    currency: string;
  } | null;
  payoutRecord: PayoutRecord | null;
  timeline: TimelineEvent[];
  metadata?: Record<string, unknown>;
}

export interface PayoutRecord {
  id: string;
  provider: string;
  status: string;
  amount: number;
  currency: string;
  bankTrackingNumber: string | null;
  bankReferenceId: string | null;
  paidAt: string | null;
  createdAt: string;
  responsePayload?: Record<string, unknown>;
}

export interface TimelineEvent {
  status: string;
  timestamp: string;
  label: string;
}

export interface WithdrawalActionRequest {
  note?: string;
  reason?: string;
  releaseFunds?: boolean;
  paidAt?: string;
  bankTrackingNumber?: string;
  bankReferenceId?: string;
}

export interface WithdrawalActionResult {
  success: boolean;
  status: string;
  message: string;
  approvalRequestId?: string;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listWithdrawals(params?: Record<string, unknown>) {
  return get<{ items: WithdrawalItem[]; total: number }>('/api/v1/admin/withdrawal-requests', params);
}

export async function getWithdrawalQueue(params?: Record<string, unknown>) {
  return get<{ items: WithdrawalItem[]; total: number; countsByStatus: Record<string, number> }>('/api/v1/admin/withdrawal-requests', params);
}

export async function getWithdrawal(withdrawalRequestId: string) {
  return get<WithdrawalDetail>(`/api/v1/admin/withdrawal-requests/${withdrawalRequestId}`);
}

export async function approveWithdrawal(withdrawalRequestId: string, req?: WithdrawalActionRequest) {
  return post<WithdrawalActionResult>(`/api/v1/admin/withdrawal-requests/${withdrawalRequestId}/approve`, req);
}

export async function rejectWithdrawal(withdrawalRequestId: string, req: WithdrawalActionRequest) {
  return post<WithdrawalActionResult>(`/api/v1/admin/withdrawal-requests/${withdrawalRequestId}/reject`, req);
}

export async function requestWithdrawalMoreInfo(withdrawalRequestId: string, req: WithdrawalActionRequest) {
  return post<WithdrawalActionResult>(`/api/v1/admin/withdrawal-requests/${withdrawalRequestId}/request-more-info`, req);
}

export async function markWithdrawalProcessing(withdrawalRequestId: string, req?: WithdrawalActionRequest) {
  return post<WithdrawalActionResult>(`/api/v1/admin/withdrawal-requests/${withdrawalRequestId}/mark-processing`, req);
}

export async function markWithdrawalPaid(withdrawalRequestId: string, req: WithdrawalActionRequest) {
  return post<WithdrawalActionResult>(`/api/v1/admin/withdrawal-requests/${withdrawalRequestId}/mark-paid`, req);
}

export async function markWithdrawalFailed(withdrawalRequestId: string, req: WithdrawalActionRequest) {
  return post<WithdrawalActionResult>(`/api/v1/admin/withdrawal-requests/${withdrawalRequestId}/mark-failed`, req);
}
