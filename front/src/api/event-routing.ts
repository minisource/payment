import { get, post, patch, del } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export type EventRoutingRuleStatus = 'active' | 'disabled' | 'deleted';
export type EventTargetType = 'webhook' | 'notifier' | 'message_bus' | 'internal_handler';

export interface EventRoutingRuleListItem {
  id: string;
  name: string;
  eventTypes: string[];
  targetType: EventTargetType;
  status: EventRoutingRuleStatus;
  tenantId: string;
  applicationCode: string | null;
  priority: number;
  createdAt: string;
}

export interface EventRoutingRuleDetail extends EventRoutingRuleListItem {
  description: string | null;
  targetConfig: Record<string, unknown>;
  conditions?: Record<string, unknown>;
  metadata?: Record<string, unknown>;
  updatedAt: string;
}

export interface EventRoutingRuleCreateRequest {
  name: string;
  description?: string;
  eventTypes: string[];
  targetType: EventTargetType;
  targetConfig: Record<string, unknown>;
  tenantId?: string;
  applicationCode?: string;
  priority?: number;
  conditions?: Record<string, unknown>;
  metadata?: Record<string, unknown>;
}

export interface EventRoutingRuleUpdateRequest {
  name?: string;
  description?: string;
  eventTypes?: string[];
  targetType?: EventTargetType;
  targetConfig?: Record<string, unknown>;
  tenantId?: string;
  applicationCode?: string;
  priority?: number;
  conditions?: Record<string, unknown>;
  metadata?: Record<string, unknown>;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function listEventRoutingRules(params?: Record<string, unknown>) {
  return get<{ items: EventRoutingRuleListItem[]; total: number }>('/api/v1/admin/events/routing-rules', params);
}

export async function getEventRoutingRule(routingRuleId: string) {
  return get<EventRoutingRuleDetail>(`/api/v1/admin/events/routing-rules/${routingRuleId}`);
}

export async function createEventRoutingRule(req: EventRoutingRuleCreateRequest) {
  return post<{ id: string }>('/api/v1/admin/events/routing-rules', req);
}

export async function updateEventRoutingRule(routingRuleId: string, req: EventRoutingRuleUpdateRequest) {
  return patch<{ success: boolean }>(`/api/v1/admin/events/routing-rules/${routingRuleId}`, req);
}

export async function enableEventRoutingRule(routingRuleId: string) {
  return post<{ success: boolean }>(`/api/v1/admin/events/routing-rules/${routingRuleId}/enable`);
}

export async function disableEventRoutingRule(routingRuleId: string) {
  return post<{ success: boolean }>(`/api/v1/admin/events/routing-rules/${routingRuleId}/disable`);
}

export async function deleteEventRoutingRule(routingRuleId: string) {
  return del<{ success: boolean }>(`/api/v1/admin/events/routing-rules/${routingRuleId}`);
}
