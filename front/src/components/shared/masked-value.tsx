'use client';

import { cn } from '@/lib/utils';

export function MaskedCardNumber({ value, className }: { value: string | null | undefined; className?: string }) {
  if (!value) return <span className="text-muted-foreground">—</span>;
  if (value.includes('*')) return <span className={cn('font-mono text-xs', className)}>{value}</span>;
  const masked = value.length >= 8 ? value.slice(0, 4) + ' **** **** ' + value.slice(-4) : '••••' + value.slice(-2);
  return <span className={cn('font-mono text-xs', className)}>{masked}</span>;
}

export function MaskedIban({ value, className }: { value: string | null | undefined; className?: string }) {
  if (!value) return <span className="text-muted-foreground">—</span>;
  if (value.includes('*') || value.includes('•')) return <span className={cn('font-mono text-xs', className)}>{value}</span>;
  const masked = value.slice(0, 2) + '••••' + value.slice(-4);
  return <span className={cn('font-mono text-xs', className)}>{masked}</span>;
}

export function MaskedAccountNumber({ value, className }: { value: string | null | undefined; className?: string }) {
  if (!value) return <span className="text-muted-foreground">—</span>;
  if (value.includes('*')) return <span className={cn('font-mono text-xs', className)}>{value}</span>;
  const masked = '••••' + value.slice(-4);
  return <span className={cn('font-mono text-xs', className)}>{masked}</span>;
}
