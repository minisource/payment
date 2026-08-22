import { get, post, patch, del } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export type WebhookSubscriptionStatus = 'active' | 'disabled' | 'deleted' | 'failing' | 'paused';

export interface WebhookSubscriptionListItem {
  id: string;
  name: string;
  targetUrl: string;
  status: WebhookSubscriptionStatus;
  eventTypes: string[];
  tenantId: string;
  applicationCode: string | null;
  lastDeliveryStatus: string | null;
  lastDeliveryAt: string | null;
  createdAt: string;
}

export interface WebhookSubscriptionDetail {
  id: string;
  name: string;
  description: string | null;
  targetUrl: string;
  status: WebhookSubscriptionStatus;
  eventTypes: string[];
  tenantId: string;
  applicationCode: string | null;
  secretConfigured: boolean;
  createdAt: string;
  updatedAt: string;
  lastDeliveryAt: string | null;
  lastSuccessAt: string | null;
  lastFailureAt: string | null;
  metadata?: Record<string, unknown>;
}

export interface WebhookSubscriptionCreateRequest {
  name: string;
  description?: string;
  targetUrl: string;
  eventTypes: string[];
  tenantId?: string;
  applicationCode?: string;
  status?: string;
  headers?: Record<string, string>;
  metadata?: Record<string, unknown>;
}

export interface WebhookSubscriptionUpdateRequest {
  name?: string;
  description?: string;
  targetUrl?: string;
  eventTypes?: string[];
  tenantId?: string;
  applicationCode?: string;
  status?: string;
  headers?: Record<string, string>;
  metadata?: Record<string, unknown>;
}

export interface WebhookSecretRegenerateResponse {
  subscriptionId: string;
  secret: string;
  rotatedAt: string;
}

export interface WebhookSubscriptionCreateResponse {
  id: string;
  secret?: string;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listWebhookSubscriptions(params?: Record<string, unknown>) {
  return get<{ items: WebhookSubscriptionListItem[]; total: number }>('/api/v1/admin/webhooks/subscriptions', params);
}

export async function getWebhookSubscription(subscriptionId: string) {
  return get<WebhookSubscriptionDetail>(`/api/v1/admin/webhooks/subscriptions/${subscriptionId}`);
}

export async function createWebhookSubscription(req: WebhookSubscriptionCreateRequest) {
  return post<WebhookSubscriptionCreateResponse>('/api/v1/admin/webhooks/subscriptions', req);
}

export async function updateWebhookSubscription(subscriptionId: string, req: WebhookSubscriptionUpdateRequest) {
  return patch<{ success: boolean }>(`/api/v1/admin/webhooks/subscriptions/${subscriptionId}`, req);
}

export async function enableWebhookSubscription(subscriptionId: string) {
  return post<{ success: boolean }>(`/api/v1/admin/webhooks/subscriptions/${subscriptionId}/enable`);
}

export async function disableWebhookSubscription(subscriptionId: string) {
  return post<{ success: boolean }>(`/api/v1/admin/webhooks/subscriptions/${subscriptionId}/disable`);
}

export async function deleteWebhookSubscription(subscriptionId: string) {
  return del<{ success: boolean }>(`/api/v1/admin/webhooks/subscriptions/${subscriptionId}`);
}

export async function regenerateWebhookSecret(subscriptionId: string) {
  return post<WebhookSecretRegenerateResponse>(`/api/v1/admin/webhooks/subscriptions/${subscriptionId}/regenerate-secret`);
}
