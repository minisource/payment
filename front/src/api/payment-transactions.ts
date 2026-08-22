import { get, post } from '@/api/client';
import type { PaymentTransaction, PaymentIntent } from '@/types/payment';

export interface PaymentTransactionListParams {
  tenantId?: string;
  paymentIntentId?: string;
  providerCode?: string;
  gatewayConfigId?: string;
  status?: string;
  currency?: string;
  amountMin?: number;
  amountMax?: number;
  authority?: string;
  referenceId?: string;
  dateFrom?: string;
  dateTo?: string;
  query?: string;
  skip?: number;
  take?: number;
}

export interface PaymentTransactionDetail extends PaymentTransaction {
  paymentIntent: PaymentIntent;
  callbackPayload?: Record<string, unknown>;
  verifyPayload?: Record<string, unknown>;
  walletPostingResult?: {
    walletId: string;
    ledgerEntryId: string;
    amount: number;
    type: string;
  };
  timeline: Array<{ status: string; timestamp: string; label: string }>;
}

export async function listPaymentTransactions(params?: PaymentTransactionListParams) {
  return get<{ items: PaymentTransaction[]; total: number }>('/api/v1/admin/payment-transactions', params as Record<string, unknown>);
}

export async function getPaymentTransaction(paymentTransactionId: string) {
  return get<PaymentTransactionDetail>(`/api/v1/admin/payment-transactions/${paymentTransactionId}`);
}

export async function manualVerifyPaymentTransaction(paymentTransactionId: string) {
  return post<{ success: boolean; message: string; status: string }>(`/api/v1/admin/payment-transactions/${paymentTransactionId}/verify`);
}
