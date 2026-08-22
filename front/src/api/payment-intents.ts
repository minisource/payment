import { get } from '@/api/client';
import type { PaymentIntent, PaymentTransaction } from '@/types/payment';

export interface PaymentIntentListParams {
  tenantId?: string;
  status?: string;
  providerCode?: string;
  gatewayConfigId?: string;
  currency?: string;
  amountMin?: number;
  amountMax?: number;
  walletBehavior?: string;
  payerWalletId?: string;
  recipientWalletId?: string;
  externalReferenceType?: string;
  externalReferenceId?: string;
  paymentLinkId?: string;
  dateFrom?: string;
  dateTo?: string;
  query?: string;
  skip?: number;
  take?: number;
}

export interface PaymentIntentDetail extends PaymentIntent {
  feeAmount: number;
  netAmount: number;
  payerWalletId: string | null;
  recipientWalletId: string | null;
  paymentLinkId: string | null;
  succeededAt: string | null;
  failedAt: string | null;
  gatewayTransactions: PaymentTransaction[];
  timeline: TimelineEvent[];
  walletPostingResult?: WalletPostingResult;
  metadata?: Record<string, unknown>;
}

export interface TimelineEvent {
  status: string;
  timestamp: string;
  label: string;
  detail?: string;
}

export interface WalletPostingResult {
  creditWalletId: string;
  debitWalletId: string;
  creditAmount: number;
  debitAmount: number;
  feeAmount: number;
  creditLedgerEntryId: string;
  debitLedgerEntryId: string;
  feeLedgerEntryId: string;
}

export async function listPaymentIntents(params?: PaymentIntentListParams) {
  return get<{ items: PaymentIntent[]; total: number }>('/api/v1/admin/payment-intents', params as Record<string, unknown>);
}

export async function getPaymentIntent(paymentIntentId: string) {
  return get<PaymentIntentDetail>(`/api/v1/admin/payment-intents/${paymentIntentId}`);
}

export async function listPaymentIntentTransactions(paymentIntentId: string, params?: { skip?: number; take?: number }) {
  return get<{ items: PaymentTransaction[]; total: number }>(`/api/v1/admin/payment-intents/${paymentIntentId}/transactions`, params as Record<string, unknown>);
}
