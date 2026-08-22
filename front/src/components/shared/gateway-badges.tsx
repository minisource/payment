'use client';

import { cn } from '@/lib/utils';

// ─── StrategyBadge ──────────────────────────────────────────────

const STRATEGY_LABELS: Record<string, string> = {
  priority: 'Priority',
  random: 'Random',
  weighted_random: 'Weighted',
};

const STRATEGY_COLORS: Record<string, string> = {
  priority: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
  random: 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400',
  weighted_random: 'bg-indigo-100 text-indigo-800 dark:bg-indigo-900/30 dark:text-indigo-400',
};

export function StrategyBadge({ strategy, className }: { strategy: string; className?: string }) {
  return (
    <span className={cn('inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium', STRATEGY_COLORS[strategy] || 'bg-gray-100', className)}>
      {STRATEGY_LABELS[strategy] || strategy}
    </span>
  );
}

// ─── EnvironmentBadge ──────────────────────────────────────────

const ENV_COLORS: Record<string, string> = {
  sandbox: 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400',
  production: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
};

export function EnvironmentBadge({ environment, className }: { environment: string; className?: string }) {
  return (
    <span className={cn('inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium capitalize', ENV_COLORS[environment] || 'bg-gray-100', className)}>
      {environment}
    </span>
  );
}

// ─── ScopeBadge ─────────────────────────────────────────────────

const SCOPE_COLORS: Record<string, string> = {
  global: 'bg-teal-100 text-teal-800 dark:bg-teal-900/30 dark:text-teal-400',
  tenant: 'bg-cyan-100 text-cyan-800 dark:bg-cyan-900/30 dark:text-cyan-400',
  application: 'bg-sky-100 text-sky-800 dark:bg-sky-900/30 dark:text-sky-400',
};

export function ScopeBadge({ scope, className }: { scope: string; className?: string }) {
  return (
    <span className={cn('inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium capitalize', SCOPE_COLORS[scope] || 'bg-gray-100', className)}>
      {scope}
    </span>
  );
}

// ─── GatewayHealthBadge ────────────────────────────────────────

const HEALTH_COLORS: Record<string, string> = {
  healthy: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  degraded: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
  unhealthy: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
  unknown: 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400',
};

export function GatewayHealthBadge({ health, className }: { health: string; className?: string }) {
  const color = HEALTH_COLORS[health] || HEALTH_COLORS.unknown;
  return (
    <span className={cn('inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium', color, className)}>
      <span className={cn('h-1.5 w-1.5 rounded-full', health === 'healthy' ? 'bg-green-500' : health === 'degraded' ? 'bg-yellow-500' : health === 'unhealthy' ? 'bg-red-500' : 'bg-gray-500')} />
      {health || 'unknown'}
    </span>
  );
}

// ─── ProviderBadge ─────────────────────────────────────────────

export function ProviderBadge({ provider, className }: { provider: string; className?: string }) {
  return (
    <span className={cn('inline-flex items-center rounded bg-muted px-2 py-0.5 text-xs font-mono font-medium', className)}>
      {provider}
    </span>
  );
}
