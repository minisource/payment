'use client';

import { cn } from '@/lib/utils';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface DataTablePaginationProps {
  skip: number;
  take: number;
  total: number;
  onPageChange: (skip: number) => void;
  className?: string;
}

export function DataTablePagination({ skip, take, total, onPageChange, className }: DataTablePaginationProps) {
  const currentPage = Math.floor(skip / take) + 1;
  const totalPages = Math.ceil(total / take);
  if (totalPages <= 1) return null;

  const getPageNumbers = () => {
    const pages: number[] = [];
    const maxVisible = 7;
    let start = Math.max(1, currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(totalPages, start + maxVisible - 1);
    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  };

  return (
    <div className={cn('flex items-center justify-between border-t px-4 py-3', className)}>
      <p className="text-xs text-muted-foreground">
        {skip + 1}–{Math.min(skip + take, total)} of {total}
      </p>
      <div className="flex items-center gap-1">
        <button
          disabled={skip === 0}
          onClick={() => onPageChange(Math.max(0, skip - take))}
          className="rounded p-1 hover:bg-accent disabled:opacity-30"
          title="Previous page"
        >
          <ChevronLeft className="h-4 w-4" />
        </button>
        {getPageNumbers().map((page) => (
          <button
            key={page}
            onClick={() => onPageChange((page - 1) * take)}
            className={cn('rounded px-2 py-1 text-xs', page === currentPage ? 'bg-primary text-primary-foreground' : 'hover:bg-accent')}
          >
            {page}
          </button>
        ))}
        <button
          disabled={skip + take >= total}
          onClick={() => onPageChange(skip + take)}
          className="rounded p-1 hover:bg-accent disabled:opacity-30"
          title="Next page"
        >
          <ChevronRight className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}
