import { apiClient } from '@/api/client';
import type { PaymentIntentDto, PaymentIntentDetailDto, PaymentTransactionDto, ListPaymentIntentsParams, ListPaymentIntentsResponse } from '../types/payment-intent.types';

export async function listPaymentIntents(params?: ListPaymentIntentsParams, signal?: AbortSignal): Promise<ListPaymentIntentsResponse> {
  const { data } = await apiClient.get<ListPaymentIntentsResponse>('/api/v1/admin/payment-intents', { params, signal });
  return data;
}

export async function getPaymentIntent(paymentIntentId: string, signal?: AbortSignal): Promise<PaymentIntentDetailDto> {
  const { data } = await apiClient.get<PaymentIntentDetailDto>(`/api/v1/admin/payment-intents/${paymentIntentId}`, { signal });
  return data;
}

export async function listPaymentIntentTransactions(
  paymentIntentId: string,
  params?: { skip?: number; take?: number },
  signal?: AbortSignal,
): Promise<ListPaymentIntentsResponse> {
  const { data } = await apiClient.get<ListPaymentIntentsResponse>(
    `/api/v1/admin/payment-intents/${paymentIntentId}/transactions`,
    { params, signal },
  );
  return data;
}
