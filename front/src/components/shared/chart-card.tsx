'use client';

import { cn } from '@/lib/utils';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/shared/states';
import { BarChart3 } from 'lucide-react';
import React from 'react';

interface ChartCardProps {
  title: string;
  children?: React.ReactNode;
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyMessage?: string;
  isError?: boolean;
  errorMessage?: string;
  className?: string;
  onRetry?: () => void;
}

export function ChartCard({
  title, children, isLoading, isEmpty, emptyMessage = 'No data available',
  isError, errorMessage, className, onRetry,
}: ChartCardProps) {
  return (
    <Card className={cn(className)}>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="flex h-[200px] items-center justify-center">
            <Skeleton className="h-[160px] w-full" />
          </div>
        ) : isError ? (
          <div className="flex h-[200px] flex-col items-center justify-center space-y-2 text-center">
            <p className="text-sm text-destructive">{errorMessage || 'Failed to load chart'}</p>
            {onRetry && (
              <button onClick={onRetry} className="text-xs text-primary hover:underline">Retry</button>
            )}
          </div>
        ) : isEmpty ? (
          <div className="flex h-[200px] flex-col items-center justify-center space-y-2 text-center">
            <BarChart3 className="h-8 w-8 text-muted-foreground/50" />
            <p className="text-sm text-muted-foreground">{emptyMessage}</p>
          </div>
        ) : (
          <div className="h-[200px]">{children}</div>
        )}
      </CardContent>
    </Card>
  );
}
