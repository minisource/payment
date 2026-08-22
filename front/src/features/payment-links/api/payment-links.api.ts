import { apiClient } from '@/api/client';
import type { PaymentLinkDetailDto, ListPaymentLinksParams, ListPaymentLinksResponse } from '../types/payment-links.types';

export async function listPaymentLinks(params: ListPaymentLinksParams, signal?: AbortSignal) {
  const { data } = await apiClient.get<ListPaymentLinksResponse>('/api/v1/admin/payment-links', {
    params,
    signal,
  });
  return data;
}

export async function getPaymentLink(paymentLinkId: string, signal?: AbortSignal) {
  const { data } = await apiClient.get<PaymentLinkDetailDto>(`/api/v1/admin/payment-links/${paymentLinkId}`, {
    signal,
  });
  return data;
}

export async function pausePaymentLink(paymentLinkId: string) {
  const { data } = await apiClient.post<{ success: boolean }>(`/api/v1/admin/payment-links/${paymentLinkId}/pause`);
  return data;
}

export async function resumePaymentLink(paymentLinkId: string) {
  const { data } = await apiClient.post<{ success: boolean }>(`/api/v1/admin/payment-links/${paymentLinkId}/resume`);
  return data;
}

export async function disablePaymentLink(paymentLinkId: string, reason?: string) {
  const { data } = await apiClient.post<{ success: boolean }>(`/api/v1/admin/payment-links/${paymentLinkId}/disable`, { reason });
  return data;
}
