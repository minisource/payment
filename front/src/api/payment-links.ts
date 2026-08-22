import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface PaymentLinkItem {
  id: string;
  tenantId: string;
  applicationCode: string | null;
  ownerType: string;
  ownerId: string;
  title: string;
  description: string | null;
  amountType: 'fixed' | 'free' | 'minimum';
  amount: number | null;
  currency: string;
  status: string;
  maxUses: number | null;
  usedCount: number;
  totalPaid: number;
  safePublicUrl: string | null;
  expiresAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PaymentLinkDetail extends PaymentLinkItem {
  successfulPayments: Array<{ id: string; amount: number; currency: string; createdAt: string }>;
  relatedPaymentIntents: Array<{ id: string; amount: number; currency: string; status: string }>;
  metadata?: Record<string, unknown>;
}

export async function listPaymentLinks(params?: Record<string, unknown>) {
  return get<{ items: PaymentLinkItem[]; total: number }>('/api/v1/admin/payment-links', params);
}

export async function getPaymentLink(paymentLinkId: string) {
  return get<PaymentLinkDetail>(`/api/v1/admin/payment-links/${paymentLinkId}`);
}

export async function pausePaymentLink(paymentLinkId: string) {
  return post<{ success: boolean }>(`/api/v1/admin/payment-links/${paymentLinkId}/pause`);
}

export async function resumePaymentLink(paymentLinkId: string) {
  return post<{ success: boolean }>(`/api/v1/admin/payment-links/${paymentLinkId}/resume`);
}

export async function disablePaymentLink(paymentLinkId: string, reason?: string) {
  return post<{ success: boolean }>(`/api/v1/admin/payment-links/${paymentLinkId}/disable`, { reason });
}
