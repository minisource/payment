import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export type WebhookDeliveryStatus = 'pending' | 'processing' | 'delivered' | 'failed' | 'dead_lettered' | 'skipped' | 'cancelled';

export interface WebhookDeliveryListItem {
  id: string;
  subscriptionId: string;
  eventType: string;
  outboxEventId: string;
  status: WebhookDeliveryStatus;
  httpStatusCode: number | null;
  attemptCount: number;
  lastError: string | null;
  createdAt: string;
  deliveredAt: string | null;
}

export interface WebhookDeliveryDetail extends WebhookDeliveryListItem {
  maxAttempts: number;
  nextRetryAt: string | null;
  failedAt: string | null;
  updatedAt: string;
  requestHeaders?: Record<string, string>;
  requestPayload?: Record<string, unknown>;
  responseHeaders?: Record<string, string>;
  responseBody?: string | Record<string, unknown>;
  attempts: DeliveryAttempt[];
  subscriptionName?: string;
  subscriptionTargetUrl?: string;
}

export interface DeliveryAttempt {
  attempt: number;
  startedAt: string;
  completedAt: string | null;
  success: boolean;
  httpStatus: number | null;
  error: string | null;
  durationMs: number | null;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listWebhookDeliveries(params?: Record<string, unknown>) {
  return get<{ items: WebhookDeliveryListItem[]; total: number }>('/api/v1/admin/webhooks/deliveries', params);
}

export async function getWebhookDelivery(deliveryId: string) {
  return get<WebhookDeliveryDetail>(`/api/v1/admin/webhooks/deliveries/${deliveryId}`);
}

export async function retryWebhookDelivery(deliveryId: string, req?: { note?: string }) {
  return post<{ success: boolean }>(`/api/v1/admin/webhooks/deliveries/${deliveryId}/retry`, req);
}

export async function deadLetterWebhookDelivery(deliveryId: string, req: { reason: string }) {
  return post<{ success: boolean }>(`/api/v1/admin/webhooks/deliveries/${deliveryId}/dead-letter`, req);
}
