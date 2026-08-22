'use client';

import { cn } from '@/lib/utils';
import React from 'react';

interface DetailCardProps {
  title: string;
  children: React.ReactNode;
  className?: string;
  actions?: React.ReactNode;
  /** Whether to render without the inner padding */
  noPadding?: boolean;
}

export function DetailCard({ title, children, className, actions, noPadding }: DetailCardProps) {
  return (
    <div className={cn('rounded-lg border bg-card shadow-sm', className)}>
      <div className="flex items-center justify-between border-b px-6 py-3">
        <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
          {title}
        </h3>
        {actions && <div className="flex items-center gap-2">{actions}</div>}
      </div>
      <div className={cn(!noPadding && 'p-6')}>
        {children}
      </div>
    </div>
  );
}
