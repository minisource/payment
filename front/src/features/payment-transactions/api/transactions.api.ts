import { apiClient } from '@/api/client';
import type { ListPaymentTransactionsParams, ListPaymentTransactionsResponse, PaymentTransactionDetailDto } from '../types/transaction.types';

export async function listPaymentTransactions(params?: ListPaymentTransactionsParams, signal?: AbortSignal): Promise<ListPaymentTransactionsResponse> {
  const { data } = await apiClient.get<ListPaymentTransactionsResponse>('/api/v1/admin/payment-transactions', { params, signal });
  return data;
}

export async function getPaymentTransaction(transactionId: string, signal?: AbortSignal): Promise<PaymentTransactionDetailDto> {
  const { data } = await apiClient.get<PaymentTransactionDetailDto>(`/api/v1/admin/payment-transactions/${transactionId}`, { signal });
  return data;
}
