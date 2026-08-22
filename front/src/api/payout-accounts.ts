import { get, post } from '@/api/client';

export interface PayoutAccountItem {
  id: string;
  tenantId: string;
  ownerType: string;
  ownerId: string;
  accountType: 'card' | 'iban' | 'bank_account' | 'wallet' | 'other';
  holderName: string;
  bankName: string | null;
  cardNumberMasked: string | null;
  ibanMasked: string | null;
  accountNumberMasked: string | null;
  currency: string;
  status: string;
  verifiedAt: string | null;
  rejectedAt: string | null;
  rejectionReason: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PayoutAccountDetail extends PayoutAccountItem {
  verifiedByUserId: string | null;
  disabledAt: string | null;
  disableReason: string | null;
  relatedWithdrawals: Array<{ id: string; amount: number; currency: string; status: string; createdAt: string }>;
}

export async function listPayoutAccounts(params?: Record<string, unknown>) {
  return get<{ items: PayoutAccountItem[]; total: number }>('/api/v1/admin/payout-accounts', params);
}

export async function getPayoutAccount(payoutAccountId: string) {
  return get<PayoutAccountDetail>(`/api/v1/admin/payout-accounts/${payoutAccountId}`);
}

export async function verifyPayoutAccount(payoutAccountId: string) {
  return post<{ success: boolean }>(`/api/v1/admin/payout-accounts/${payoutAccountId}/verify`);
}

export async function rejectPayoutAccount(payoutAccountId: string, reason: string) {
  return post<{ success: boolean }>(`/api/v1/admin/payout-accounts/${payoutAccountId}/reject`, { reason });
}

export async function disablePayoutAccount(payoutAccountId: string, reason: string) {
  return post<{ success: boolean }>(`/api/v1/admin/payout-accounts/${payoutAccountId}/disable`, { reason });
}
