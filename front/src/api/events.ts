import { get } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface EventsOverview {
  pendingOutboxEvents: number;
  processingOutboxEvents: number;
  failedOutboxEvents: number;
  deadLetteredEvents: number;
  activeWebhookSubscriptions: number;
  failedWebhookDeliveries: number;
  averageDeliveryLatencyMs: number | null;
  dispatcherStatus: string | null;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function getEventsOverview() {
  return get<EventsOverview>('/api/v1/admin/reports/events/overview');
}
