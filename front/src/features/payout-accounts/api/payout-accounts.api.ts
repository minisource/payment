import { apiClient } from '@/api/client';
import type { PayoutAccountItemDto, ListPayoutAccountsParams, ListPayoutAccountsResponse } from '../types/payout-accounts.types';

export async function listPayoutAccounts(params: ListPayoutAccountsParams, signal?: AbortSignal) {
  const { data } = await apiClient.get<ListPayoutAccountsResponse>('/api/v1/admin/payout-accounts', {
    params,
    signal,
  });
  return data;
}

export async function getPayoutAccount(payoutAccountId: string, signal?: AbortSignal) {
  const { data } = await apiClient.get<PayoutAccountItemDto>(`/api/v1/admin/payout-accounts/${payoutAccountId}`, {
    signal,
  });
  return data;
}

export async function verifyPayoutAccount(payoutAccountId: string) {
  const { data } = await apiClient.post<{ success: boolean }>(`/api/v1/admin/payout-accounts/${payoutAccountId}/verify`);
  return data;
}

export async function rejectPayoutAccount(payoutAccountId: string, reason: string) {
  const { data } = await apiClient.post<{ success: boolean }>(`/api/v1/admin/payout-accounts/${payoutAccountId}/reject`, { reason });
  return data;
}

export async function disablePayoutAccount(payoutAccountId: string, reason: string) {
  const { data } = await apiClient.post<{ success: boolean }>(`/api/v1/admin/payout-accounts/${payoutAccountId}/disable`, { reason });
  return data;
}
