import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface LimitUsageItem {
  id: string;
  tenantId: string;
  subjectType: string;
  subjectId: string;
  operationType: string;
  windowType: string;
  usedAmount: number;
  limitAmount: number;
  remainingAmount: number;
  usedCount: number;
  limitCount: number;
  currency: string;
  status: string;
  resetAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface LimitUsageActionResult {
  success: boolean;
  message: string;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listLimitUsage(params?: Record<string, unknown>) {
  return get<{ items: LimitUsageItem[]; total: number }>('/api/v1/admin/reports/financial-security/limit-usage', params);
}

export async function getLimitUsage(usageId: string) {
  return get<LimitUsageItem>(`/api/v1/admin/reports/financial-security/limit-usage/${usageId}`);
}

export async function resetLimitUsage(usageId: string, req: { reason: string }) {
  return post<LimitUsageActionResult>(`/api/v1/admin/reports/financial-security/limit-usage/${usageId}/reset`, req);
}

export async function increaseTemporaryLimit(usageId: string, req: { reason: string; newLimitAmount?: number }) {
  return post<LimitUsageActionResult>(`/api/v1/admin/reports/financial-security/limit-usage/${usageId}/temporary-limit`, req);
}
