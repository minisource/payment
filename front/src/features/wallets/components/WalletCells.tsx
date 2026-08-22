'use client';

import { cn } from '@/lib/utils';
import type { WalletDto } from '../types/wallet.types';

// ─── Status Badge ────────────────────────────────────────

const STATUS_STYLES: Record<string, string> = {
  active: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  frozen: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
  disabled: 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400',
  suspended: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
  deleted: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
};

export function WalletStatusBadge({ status, className }: { status: string; className?: string }) {
  const style = STATUS_STYLES[status] || STATUS_STYLES.disabled;
  return (
    <span className={cn('inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium', style, className)}>
      {status}
    </span>
  );
}

// ─── Owner Cell ──────────────────────────────────────────

export function WalletOwnerCell({ wallet, className }: { wallet: WalletDto; className?: string }) {
  return (
    <div className={cn('flex flex-col', className)}>
      <span className="text-xs font-medium">{wallet.owner_type}</span>
      <span className="font-mono text-xs text-muted-foreground">{wallet.owner_id?.slice(0, 12)}…</span>
    </div>
  );
}

// ─── Balance Cell ────────────────────────────────────────

const CURRENCY_SYMBOLS: Record<string, string> = {
  IRT: 'تومان',
  IRR: '﷼',
  USD: '$',
  EUR: '€',
};

function formatMoney(amount: string, currency: string): string {
  const num = parseFloat(amount);
  if (isNaN(num)) return '—';
  const formatted = new Intl.NumberFormat('en-US').format(Math.abs(num));
  const symbol = CURRENCY_SYMBOLS[currency] || currency;
  return `${formatted} ${symbol}`;
}

export function WalletBalanceCell({ wallet, className }: { wallet: WalletDto; className?: string }) {
  return (
    <div className={cn('flex flex-col', className)}>
      <span className="font-mono text-xs font-medium tabular-nums">
        {formatMoney(wallet.available_balance, wallet.currency)}
      </span>
      {parseFloat(wallet.locked_balance) > 0 && (
        <span className="font-mono text-[10px] text-muted-foreground tabular-nums">
          Locked: {formatMoney(wallet.locked_balance, wallet.currency)}
        </span>
      )}
    </div>
  );
}
