'use client';

import { cn } from '@/lib/utils';
import { Search, X, RefreshCw, Calendar, Download } from 'lucide-react';
import React from 'react';

interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
}

export function SearchInput({ value, onChange, placeholder = 'Search...', className }: SearchInputProps) {
  return (
    <div className={cn('relative', className)}>
      <Search className="absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="h-9 w-full rounded-md border bg-background pl-8 pr-8 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/20"
      />
      {value && (
        <button onClick={() => onChange('')} className="absolute right-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground">
          <X className="h-4 w-4" />
        </button>
      )}
    </div>
  );
}

interface DateRangeFilterProps {
  dateFrom?: string;
  dateTo?: string;
  onChange: (from?: string, to?: string) => void;
  className?: string;
}

export function DateRangeFilter({ dateFrom, dateTo, onChange, className }: DateRangeFilterProps) {
  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Calendar className="h-4 w-4 text-muted-foreground" />
      <input
        type="date"
        value={dateFrom || ''}
        onChange={(e) => onChange(e.target.value || undefined, dateTo)}
        className="h-9 rounded-md border bg-background px-2 text-xs"
        title="From date"
      />
      <span className="text-xs text-muted-foreground">to</span>
      <input
        type="date"
        value={dateTo || ''}
        onChange={(e) => onChange(dateFrom, e.target.value || undefined)}
        className="h-9 rounded-md border bg-background px-2 text-xs"
        title="To date"
      />
      {(dateFrom || dateTo) && (
        <button onClick={() => onChange(undefined, undefined)} className="text-muted-foreground hover:text-foreground">
          <X className="h-4 w-4" />
        </button>
      )}
    </div>
  );
}

interface RefreshButtonProps {
  onClick: () => void;
  isLoading?: boolean;
  lastRefreshed?: Date;
  className?: string;
}

export function RefreshButton({ onClick, isLoading, lastRefreshed, className }: RefreshButtonProps) {
  return (
    <div className={cn('flex items-center gap-2', className)}>
      {lastRefreshed && (
        <span className="text-xs text-muted-foreground">
          Last: {lastRefreshed.toLocaleTimeString()}
        </span>
      )}
      <button
        onClick={onClick}
        disabled={isLoading}
        className="inline-flex items-center gap-1 rounded-md border px-3 py-1.5 text-xs font-medium hover:bg-accent disabled:opacity-50"
      >
        <RefreshCw className={cn('h-3.5 w-3.5', isLoading && 'animate-spin')} />
        Refresh
      </button>
    </div>
  );
}

interface ExportButtonProps {
  onClick?: () => void;
  disabled?: boolean;
  disabledReason?: string;
  className?: string;
}

export function ExportButton({ onClick, disabled, disabledReason, className }: ExportButtonProps) {
  return (
    <button
      onClick={onClick}
      disabled={disabled || !onClick}
      title={disabled ? disabledReason || 'Export not available' : 'Export data'}
      className={cn(
        'inline-flex items-center gap-1 rounded-md border px-3 py-1.5 text-xs font-medium hover:bg-accent disabled:cursor-not-allowed disabled:opacity-40',
        className,
      )}
    >
      <Download className="h-3.5 w-3.5" />
      Export
    </button>
  );
}

interface FilterBarProps {
  children: React.ReactNode;
  className?: string;
}

export function FilterBar({ children, className }: FilterBarProps) {
  return (
    <div className={cn('flex flex-wrap items-center gap-3 rounded-md border bg-muted/30 p-3', className)}>
      {children}
    </div>
  );
}
