import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface ReconciliationBatch {
  id: string;
  batchType: string;
  tenantId: string;
  applicationCode: string | null;
  status: string;
  totalItems: number;
  matchedCount: number;
  mismatchedCount: number;
  unresolvedCount: number;
  startedAt: string | null;
  completedAt: string | null;
  createdBy: string;
  createdAt: string;
}

export interface ReconciliationBatchDetail extends ReconciliationBatch {
  runParameters?: Record<string, unknown>;
  errorDetails?: string | null;
  metadata?: Record<string, unknown>;
}

export interface ReconciliationItem {
  id: string;
  batchId: string;
  itemType: string;
  tenantId: string;
  status: string;
  severity: string;
  expectedAmount: number;
  actualAmount: number;
  differenceAmount: number;
  currency: string;
  referenceType: string;
  referenceId: string | null;
  walletId: string | null;
  paymentIntentId: string | null;
  paymentTransactionId: string | null;
  detectedAt: string;
  resolvedAt: string | null;
}

export interface ReconciliationItemDetail extends ReconciliationItem {
  expectedValues?: Record<string, unknown>;
  actualValues?: Record<string, unknown>;
  detectionReason: string | null;
  resolutionNote: string | null;
  resolvedBy: string | null;
  metadata?: Record<string, unknown>;
}

export interface RunReconciliationRequest {
  batchType: string;
  tenantId?: string;
  applicationCode?: string;
  dateFrom?: string;
  dateTo?: string;
  walletId?: string;
}

export interface ReconciliationActionResult {
  success: boolean;
  batchId?: string;
  message: string;
  approvalRequestId?: string;
}

// ═══════════════════════════════════════════════════════════════
// Batches
// ═══════════════════════════════════════════════════════════════

export async function listReconciliationBatches(params?: Record<string, unknown>) {
  return get<{ items: ReconciliationBatch[]; total: number }>('/api/v1/admin/reconciliation/batches', params);
}

export async function getReconciliationBatch(batchId: string) {
  return get<ReconciliationBatchDetail>(`/api/v1/admin/reconciliation/batches/${batchId}`);
}

export async function runReconciliationBatch(req: RunReconciliationRequest) {
  return post<ReconciliationActionResult>('/api/v1/admin/reconciliation/batches/run', req);
}

export async function cancelReconciliationBatch(batchId: string) {
  return post<ReconciliationActionResult>(`/api/v1/admin/reconciliation/batches/${batchId}/cancel`);
}

// ═══════════════════════════════════════════════════════════════
// Items
// ═══════════════════════════════════════════════════════════════

export async function listReconciliationItems(params?: Record<string, unknown>) {
  return get<{ items: ReconciliationItem[]; total: number }>('/api/v1/admin/reconciliation/items', params);
}

export async function getReconciliationItem(itemId: string) {
  return get<ReconciliationItemDetail>(`/api/v1/admin/reconciliation/items/${itemId}`);
}

export async function acknowledgeReconciliationItem(itemId: string) {
  return post<ReconciliationActionResult>(`/api/v1/admin/reconciliation/items/${itemId}/acknowledge`);
}

export async function resolveReconciliationItem(itemId: string, req: { reason: string }) {
  return post<ReconciliationActionResult>(`/api/v1/admin/reconciliation/items/${itemId}/resolve`, req);
}

export async function markReconciliationItemFalsePositive(itemId: string, req: { reason: string }) {
  return post<ReconciliationActionResult>(`/api/v1/admin/reconciliation/items/${itemId}/false-positive`, req);
}

export async function ignoreReconciliationItem(itemId: string, req: { reason: string }) {
  return post<ReconciliationActionResult>(`/api/v1/admin/reconciliation/items/${itemId}/ignore`, req);
}
