'use client';

import { cn } from '@/lib/utils';

type BadgeVariant = 'default' | 'success' | 'warning' | 'danger' | 'info' | 'neutral';

const variantStyles: Record<BadgeVariant, string> = {
  default: 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400',
  success: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  warning: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
  danger: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
  info: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
  neutral: 'bg-slate-100 text-slate-800 dark:bg-slate-900/30 dark:text-slate-400',
};

/** Complete status-to-variant mappings for all payment domain entities */
const STATUS_MAP: Record<string, BadgeVariant> = {
  // Payment intent
  pending: 'warning',
  started: 'info',
  requires_action: 'warning',
  succeeded: 'success',
  cancelled: 'neutral',
  expired: 'neutral',
  // Gateway transaction
  redirect_required: 'info',
  callback_received: 'info',
  verified: 'success',
  // Wallet
  active: 'success',
  frozen: 'warning',
  disabled: 'neutral',
  deleted: 'danger',
  // Withdrawal
  pending_review: 'warning',
  approved: 'success',
  rejected: 'danger',
  more_info_required: 'info',
  processing_payout: 'info',
  paid: 'success',
  // Risk
  low: 'info',
  medium: 'warning',
  high: 'danger',
  critical: 'danger',
  // Reconciliation
  matched: 'success',
  mismatched: 'danger',
  unresolved: 'warning',
  resolved: 'success',
  false_positive: 'neutral',
  // Outbox
  processing: 'info',
  processed: 'success',
  failed: 'danger',
  dead_lettered: 'danger',
  skipped: 'neutral',
  // Webhook delivery
  delivered: 'success',
  // Approval
  executed: 'success',
  // General states
  open: 'warning',
  completed: 'success',
};

interface EnhancedBadgeProps {
  status: string;
  className?: string;
  /** Override variant for unknown statuses */
  fallbackVariant?: BadgeVariant;
}

export function StatusBadge({ status, className, fallbackVariant = 'default' }: EnhancedBadgeProps) {
  const variant = STATUS_MAP[status] || fallbackVariant;
  const display = status.replace(/_/g, ' ');

  return (
    <span className={cn('inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium capitalize', variantStyles[variant], className)}>
      {display}
    </span>
  );
}

/** StatusBadge specifically for risk severity levels */
export function RiskSeverityBadge({ severity, className }: { severity: string; className?: string }) {
  const colors: Record<string, string> = {
    low: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
    medium: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
    high: 'bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-400',
    critical: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
  };
  const color = colors[severity] || colors.low;
  return (
    <span className={cn('inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium capitalize', color, className)}>
      {severity}
    </span>
  );
}
