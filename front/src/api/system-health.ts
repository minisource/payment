import { get } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export type HealthStatus = 'healthy' | 'degraded' | 'unhealthy' | 'unknown';

export interface SystemHealth {
  status: HealthStatus;
  uptime: string | null;
  version: string | null;
  checkedAt: string;
}

export interface HealthCheck {
  name: string;
  status: HealthStatus;
  responseTimeMs: number | null;
  checkedAt: string;
  details?: Record<string, unknown>;
}

export interface DependencyHealth {
  name: string;
  type: string;
  status: HealthStatus;
  responseTimeMs: number | null;
  checkedAt: string;
  error: string | null;
  details?: Record<string, unknown>;
}

export interface DispatcherStatus {
  name: string;
  type: string;
  status: HealthStatus;
  isRunning: boolean;
  pendingCount: number;
  processingCount: number;
  lastProcessedAt: string | null;
  error: string | null;
}

// ═══════════════════════════════════════════════════════════════
// API calls
// ═══════════════════════════════════════════════════════════════

export async function getSystemHealth() {
  return get<SystemHealth>('/api/v1/admin/system-health');
}

export async function getReadiness() {
  return get<{ status: HealthStatus; checks: HealthCheck[] }>('/health/ready');
}

export async function getLiveness() {
  return get<{ status: HealthStatus }>('/health/live');
}

export async function getDependencyHealth() {
  return get<{ items: DependencyHealth[] }>('/api/v1/admin/system-health/dependencies');
}

export async function getDispatcherStatus() {
  return get<{ items: DispatcherStatus[] }>('/api/v1/admin/system-health/dispatchers');
}
