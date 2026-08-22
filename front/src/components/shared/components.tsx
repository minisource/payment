'use client';

import React from 'react';
import { cn } from '@/lib/utils';

interface StatusBadgeProps {
  status: string;
  colors?: Record<string, string>;
  className?: string;
}

const defaultColors: Record<string, string> = {
  active: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  inactive: 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400',
  pending: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
  succeeded: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  failed: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
  processing: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
  completed: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  verified: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  open: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
  resolved: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  dead_lettered: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
  delivered: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  skipped: 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400',
  paid: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  rejected: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
  approved: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  disabled: 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400',
  deleted: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
};

export function StatusBadge({ status, colors, className }: StatusBadgeProps) {
  const colorMap = { ...defaultColors, ...colors };
  const colorClass = colorMap[status] || 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400';
  return (
    <span className={cn('inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium', colorClass, className)}>
      {status.replace(/_/g, ' ')}
    </span>
  );
}

export function MoneyAmount({ amount, currency = 'IRR', className }: { amount: number | string; currency?: string; className?: string }) {
  const num = typeof amount === 'string' ? parseFloat(amount) : amount;
  if (isNaN(num)) return <span className={cn('text-muted-foreground', className)}>—</span>;
  const formatted = new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(num);
  return <span className={cn('font-mono tabular-nums', className)}>{formatted} {currency}</span>;
}

export function CopyText({ text }: { text: string }) {
  const [copied, setCopied] = React.useState(false);
  const handleCopy = async () => {
    await navigator.clipboard.writeText(text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };
  return (
    <button onClick={handleCopy} className="inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground" title="Copy">
      <span className="truncate max-w-[200px]">{text}</span>
      {copied ? <span className="text-green-500 text-xs">✓</span> : <span className="text-xs">📋</span>}
    </button>
  );
}

export function ConfirmDialog({ open, onClose, onConfirm, title, description, confirmLabel = 'Confirm', destructive = false, confirmDisabled = false }: {
  open: boolean; onClose: () => void; onConfirm: () => void;
  title: string; description?: string | React.ReactNode; confirmLabel?: string; destructive?: boolean; confirmDisabled?: boolean;
}) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="fixed inset-0 bg-black/50" onClick={onClose} />
      <div className="relative z-50 w-full max-w-md rounded-lg border bg-background p-6 shadow-lg">
        <h3 className="text-lg font-semibold">{title}</h3>
        {description && <div className="mt-2 text-sm text-muted-foreground">{description}</div>}
        <div className="mt-4 flex justify-end gap-3">
          <button onClick={onClose} className="rounded-md border px-4 py-2 text-sm hover:bg-accent">Cancel</button>
          <button onClick={() => { onConfirm(); onClose(); }} disabled={confirmDisabled}
            className={cn('rounded-md px-4 py-2 text-sm text-white disabled:opacity-50',
              destructive ? 'bg-destructive hover:bg-destructive/90' : 'bg-primary hover:bg-primary/90')}>
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
