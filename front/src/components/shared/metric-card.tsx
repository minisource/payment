'use client';

import { cn } from '@/lib/utils';
import { Card, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/shared/states';
import { LucideIcon, TrendingDown, TrendingUp } from 'lucide-react';

interface MetricCardProps {
  title: string;
  value?: string | number | React.ReactNode;
  icon?: LucideIcon;
  trend?: { value: string; direction: 'up' | 'down' };
  subtext?: string;
  isLoading?: boolean;
  isError?: boolean;
  errorMessage?: string;
  className?: string;
  onClick?: () => void;
}

export function MetricCard({
  title, value, icon: Icon, trend, subtext,
  isLoading, isError, errorMessage, className, onClick,
}: MetricCardProps) {
  if (isError) {
    return (
      <Card className={cn('border-destructive/50', className, onClick && 'cursor-pointer')} onClick={onClick}>
        <CardContent className="p-4">
          <p className="text-xs font-medium text-muted-foreground">{title}</p>
          <p className="mt-1 text-sm text-destructive">{errorMessage || 'Failed to load'}</p>
        </CardContent>
      </Card>
    );
  }

  if (isLoading) {
    return (
      <Card className={className}>
        <CardContent className="p-4 space-y-2">
          <Skeleton className="h-3 w-20" />
          <Skeleton className="h-7 w-28" />
          <Skeleton className="h-3 w-16" />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={cn('transition-shadow hover:shadow-md', className, onClick && 'cursor-pointer')} onClick={onClick}>
      <CardContent className="p-4">
        <div className="flex items-center justify-between">
          <p className="text-xs font-medium text-muted-foreground">{title}</p>
          {Icon && <Icon className="h-4 w-4 text-muted-foreground" />}
        </div>
        <p className="mt-1 text-2xl font-bold tabular-nums">{value ?? '—'}</p>
        {(trend || subtext) && (
          <div className="mt-1 flex items-center gap-1.5">
            {trend && (
              <span className={cn(
                'inline-flex items-center gap-0.5 text-xs font-medium',
                trend.direction === 'up' ? 'text-green-600' : 'text-red-600',
              )}>
                {trend.direction === 'up' ? <TrendingUp className="h-3 w-3" /> : <TrendingDown className="h-3 w-3" />}
                {trend.value}
              </span>
            )}
            {subtext && <span className="text-xs text-muted-foreground">{subtext}</span>}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
