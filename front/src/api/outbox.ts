import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export type OutboxEventStatus = 'pending' | 'processing' | 'processed' | 'failed' | 'dead_lettered' | 'skipped' | 'cancelled';

export interface OutboxEventListItem {
  id: string;
  eventType: string;
  status: OutboxEventStatus;
  aggregateType: string;
  aggregateId: string;
  tenantId: string;
  applicationCode: string | null;
  attemptCount: number;
  nextRetryAt: string | null;
  createdAt: string;
  processedAt: string | null;
  requestId: string | null;
  correlationId: string | null;
}

export interface DeliveryAttempt {
  attempt: number;
  startedAt: string;
  completedAt: string | null;
  success: boolean;
  error: string | null;
}

export interface OutboxEventDetail extends OutboxEventListItem {
  payloadVersion: number;
  maxAttempts: number;
  availableAt: string | null;
  failedAt: string | null;
  deadLetteredAt: string | null;
  skippedAt: string | null;
  updatedAt: string;
  payload?: Record<string, unknown>;
  deliveryAttempts: DeliveryAttempt[];
  errorMessage: string | null;
}

export interface OutboxActionResult {
  success: boolean;
  message: string;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listOutboxEvents(params?: Record<string, unknown>) {
  return get<{ items: OutboxEventListItem[]; total: number }>('/api/v1/admin/outbox/events', params);
}

export async function getOutboxEvent(eventId: string) {
  return get<OutboxEventDetail>(`/api/v1/admin/outbox/events/${eventId}`);
}

export async function retryOutboxEvent(eventId: string, req?: { note?: string }) {
  return post<OutboxActionResult>(`/api/v1/admin/outbox/events/${eventId}/retry`, req);
}

export async function skipOutboxEvent(eventId: string, req: { reason: string }) {
  return post<OutboxActionResult>(`/api/v1/admin/outbox/events/${eventId}/skip`, req);
}

export async function deadLetterOutboxEvent(eventId: string, req: { reason: string }) {
  return post<OutboxActionResult>(`/api/v1/admin/outbox/events/${eventId}/dead-letter`, req);
}
