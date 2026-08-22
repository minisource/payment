'use client';

import { Card, CardContent, CardTitle } from '@/components/ui/card';
import { DateRangeFilter, FilterBar, SearchInput, ExportButton } from '@/components/shared/filters';
import { useState } from 'react';

export function ReportShell({ title, description, permission }: { title: string; description: string; permission: string }) {
  const [search, setSearch] = useState('');
  const [dateFrom, setDateFrom] = useState<string | undefined>();
  const [dateTo, setDateTo] = useState<string | undefined>();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
        <p className="text-sm text-muted-foreground">{description}</p>
        <p className="mt-1 text-xs text-muted-foreground">Permission required: <code className="rounded bg-muted px-1 font-mono">{permission}</code></p>
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <DateRangeFilter dateFrom={dateFrom} dateTo={dateTo} onChange={(f, t) => { setDateFrom(f); setDateTo(t); }} />
        <ExportButton disabled disabledReason="Export API not yet available — Coming in later phase" />
      </FilterBar>

      <Card className="border-dashed">
        <CardContent className="flex min-h-[300px] flex-col items-center justify-center p-8 text-center">
          <CardTitle className="text-lg font-medium text-muted-foreground">Coming in a later phase</CardTitle>
          <p className="mt-2 max-w-md text-sm text-muted-foreground">
            This report view will include detailed filters, charts, data tables, and export functionality.
            The backend API endpoints for reports are being documented in <code className="rounded bg-muted px-1 font-mono">docs/api-gaps.md</code>.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
