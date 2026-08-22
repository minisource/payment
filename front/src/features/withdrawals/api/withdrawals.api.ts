import { apiClient } from '@/api/client';
import type { WithdrawalItem, ListWithdrawalsParams, ListWithdrawalsResponse } from '../types/withdrawal.types';

export async function listWithdrawals(params?: ListWithdrawalsParams, signal?: AbortSignal): Promise<ListWithdrawalsResponse> {
  const { data } = await apiClient.get<ListWithdrawalsResponse>('/api/v1/admin/withdrawal-requests', { params, signal });
  return data;
}

export async function getWithdrawal(withdrawalId: string, signal?: AbortSignal): Promise<WithdrawalItem> {
  const { data } = await apiClient.get<WithdrawalItem>(`/api/v1/admin/withdrawal-requests/${withdrawalId}`, { signal });
  return data;
}
