'use client';

import { cn } from '@/lib/utils';
import { Skeleton } from '@/components/shared/states';

interface MoneyAmountProps {
  amount: number | string;
  currency?: string;
  className?: string;
  /** Visual tone for negative values */
  tone?: 'default' | 'danger' | 'success';
  isLoading?: boolean;
  /** Show raw amount on click-to-copy tooltip */
  copyable?: boolean;
}

const CURRENCY_SYMBOLS: Record<string, string> = {
  IRT: 'تومان',
  IRR: '﷼',
  USD: '$',
  EUR: '€',
};

export function MoneyAmount({ amount, currency = 'IRR', className, tone, isLoading, copyable }: MoneyAmountProps) {
  if (isLoading) {
    return <Skeleton className={cn('h-5 w-24', className)} />;
  }

  const num = typeof amount === 'string' ? parseFloat(amount) : amount;
  if (isNaN(num)) {
    return <span className={cn('text-muted-foreground', className)}>—</span>;
  }

  const isNegative = num < 0;
  const absNum = Math.abs(num);

  const formatted = new Intl.NumberFormat('en-US', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(absNum);

  const symbol = CURRENCY_SYMBOLS[currency] || currency;

  const toneClass = tone === 'danger' || (tone !== 'success' && isNegative)
    ? 'text-red-600 dark:text-red-400'
    : tone === 'success'
      ? 'text-green-600 dark:text-green-400'
      : '';

  const content = (
    <span className={cn('font-mono tabular-nums', toneClass, className)}>
      {isNegative && <span className="mr-0.5">−</span>}
      {formatted} {symbol}
    </span>
  );

  if (copyable) {
    return (
      <button
        className="cursor-copy hover:underline decoration-dotted underline-offset-2"
        title={`Copy: ${absNum}`}
        onClick={() => navigator.clipboard.writeText(String(absNum))}
      >
        {content}
      </button>
    );
  }

  return content;
}
