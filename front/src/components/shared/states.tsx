import { cn } from '@/lib/utils';
import { Loader2, Inbox, AlertCircle, PackageOpen, Search } from 'lucide-react';
import React from 'react';
import { useT } from '@/shared/i18n/LanguageProvider';

export function Skeleton({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('animate-pulse rounded-md bg-primary/10', className)} {...props} />;
}

// ─── Loading Spinner ─────────────────────────────────

export function Loading({ className }: { className?: string }) {
  return (
    <div className={cn('flex min-h-[200px] items-center justify-center', className)}>
      <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
    </div>
  );
}

// ─── Card Skeleton (for detail pages) ───────────────

export function CardSkeleton({ rows = 4, className }: { rows?: number; className?: string }) {
  return (
    <div className={cn('space-y-4', className)}>
      {/* Header skeleton */}
      <div className="flex items-center justify-between">
        <Skeleton className="h-6 w-32" />
        <Skeleton className="h-4 w-20 rounded-full" />
      </div>
      {/* Grid skeleton */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {Array.from({ length: rows }).map((_, i) => (
          <div key={i} className="space-y-2">
            <Skeleton className="h-3 w-16" />
            <Skeleton className="h-5 w-24" />
          </div>
        ))}
      </div>
      {/* Description line skeleton */}
      <Skeleton className="h-4 w-3/4" />
    </div>
  );
}

// ─── Detail Page Skeleton (full page shimmer) ──────

export function DetailPageSkeleton({ cards = 3 }: { cards?: number }) {
  return (
    <div className="space-y-6">
      {/* Back button + title skeleton */}
      <div className="flex items-center gap-3">
        <Skeleton className="h-8 w-20" />
        <Skeleton className="h-8 w-40" />
      </div>
      {/* Card skeletons */}
      {Array.from({ length: cards }).map((_, i) => (
        <div key={i} className="rounded-lg border p-6">
          <CardSkeleton rows={4} />
        </div>
      ))}
    </div>
  );
}

// ─── Empty State ────────────────────────────────────

export function EmptyState({ title, description, icon, action }: {
  title: string;
  description?: string;
  icon?: 'inbox' | 'package' | 'search' | React.ReactNode;
  action?: { label: string; onClick: () => void };
}) {
  const Icon = icon === 'inbox' ? Inbox
    : icon === 'package' ? PackageOpen
    : icon === 'search' ? Search
    : null;

  return (
    <div className="flex min-h-[200px] flex-col items-center justify-center text-center px-4">
      {Icon && <Icon className="h-10 w-10 text-muted-foreground/50 mb-3" />}
      {!Icon && icon && <div className="mb-3 text-muted-foreground/50">{icon}</div>}
      <p className="text-lg font-medium text-muted-foreground">{title}</p>
      {description && <p className="mt-1 text-sm text-muted-foreground">{description}</p>}
      {action && (
        <button onClick={action.onClick} className="mt-4 rounded-md bg-primary px-4 py-2 text-sm text-primary-foreground hover:bg-primary/90 transition-colors">
          {action.label}
        </button>
      )}
    </div>
  );
}

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="11" cy="11" r="8" /><path d="m21 21-4.3-4.3" />
    </svg>
  );
}

// ─── Error State ────────────────────────────────────

export function ErrorState({ error, onRetry }: { error?: string; onRetry?: () => void }) {
  const { t } = useT();
  return (
    <div className="flex min-h-[200px] flex-col items-center justify-center text-center px-4">
      <AlertCircle className="h-10 w-10 text-destructive/60 mb-3" />
      <p className="text-lg font-medium text-destructive">{t('error.somethingWentWrong')}</p>
      <p className="mt-1 text-sm text-muted-foreground">{error || t('error.unexpectedError')}</p>
      {onRetry && (
        <button onClick={onRetry} className="mt-4 rounded-md bg-primary px-4 py-2 text-sm text-primary-foreground hover:bg-primary/90 transition-colors">
          {t('error.tryAgain')}
        </button>
      )}
    </div>
  );
}
