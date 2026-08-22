'use client';

import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { DataTable, DataTableColumn, Pagination } from '@/components/shared/data-table';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { FilterBar, RefreshButton, SearchInput } from '@/components/shared/filters';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { toast } from 'sonner';
import { listOutboxEvents, retryOutboxEvent, skipOutboxEvent, deadLetterOutboxEvent, type OutboxEventListItem } from '@/api/outbox';
import { Copy, RefreshCw, SkipForward, XCircle } from 'lucide-react';

const PAGE_SIZE = 20;

export default function OutboxPage() {
  return (
    <RoutePermissionGuard permissions={['payment.outbox.view_admin']}>
      <OutboxContent />
    </RoutePermissionGuard>
  );
}

function OutboxContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string; reason?: string } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['outbox', { skip, search, status: statusFilter }],
    queryFn: () => listOutboxEvents({ skip, take: PAGE_SIZE, query: search || undefined, status: statusFilter || undefined }),
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'retry') await retryOutboxEvent(action.id, action.reason ? { note: action.reason } : undefined);
      else if (action.type === 'skip') await skipOutboxEvent(action.id, { reason: reason || 'Manually skipped' });
      else if (action.type === 'deadletter') await deadLetterOutboxEvent(action.id, { reason: reason || 'Manually dead-lettered' });
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['outbox'] });
      setReason('');
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  const columns: DataTableColumn<OutboxEventListItem>[] = [
    { key: 'id', header: 'Event ID', render: (e) => <Link href={`/admin/events/outbox/${e.id}`} className="font-mono text-xs text-primary hover:underline">{e.id.slice(0, 12)}…</Link> },
    { key: 'eventType', header: 'Type', render: (e) => <span className="text-xs font-mono">{e.eventType}</span> },
    { key: 'status', header: 'Status', render: (e) => <StatusBadge status={e.status} /> },
    { key: 'aggregate', header: 'Aggregate', render: (e) => <span className="font-mono text-xs">{e.aggregateType}/{e.aggregateId?.slice(0, 8)}</span> },
    { key: 'attempts', header: 'Attempts', render: (e) => <span className="text-xs">{e.attemptCount}</span> },
    { key: 'nextRetry', header: 'Next Retry', render: (e) => <span className="text-xs">{e.nextRetryAt ? new Date(e.nextRetryAt).toLocaleString() : '—'}</span> },
    { key: 'correlationId', header: 'Correlation', render: (e) => e.correlationId ? (
      <button onClick={(ev) => { ev.stopPropagation(); navigator.clipboard.writeText(e.correlationId!); toast.success('Correlation ID copied'); }} className="font-mono text-xs text-muted-foreground hover:text-primary" title="Copy correlation ID"><Copy className="inline h-3 w-3 mr-1" />{e.correlationId.slice(0, 8)}…</button>
    ) : <span className="text-xs text-muted-foreground">—</span> },
    { key: 'created', header: 'Created', render: (e) => <span className="text-xs">{new Date(e.createdAt).toLocaleString()}</span> },
    { key: 'actions', header: '', render: (e) => (
      <div className="flex gap-1">
        {(e.status === 'failed' || e.status === 'dead_lettered') && (
          <PermissionGuard permission="payment.outbox.retry">
            <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: e.id, type: 'retry' }); }} className="text-xs text-green-600" title="Retry"><RefreshCw className="h-3 w-3" /></Button>
          </PermissionGuard>
        )}
        {e.status !== 'processed' && e.status !== 'cancelled' && (
          <PermissionGuard permission="payment.outbox.skip">
            <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: e.id, type: 'skip' }); }} className="text-xs text-yellow-600" title="Skip"><SkipForward className="h-3 w-3" /></Button>
          </PermissionGuard>
        )}
        {e.status !== 'dead_lettered' && e.status !== 'cancelled' && (
          <PermissionGuard permission="payment.outbox.dead_letter">
            <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: e.id, type: 'deadletter' }); }} className="text-xs text-red-600" title="Dead-letter"><XCircle className="h-3 w-3" /></Button>
          </PermissionGuard>
        )}
      </div>
    ) },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Outbox Events</h1>
          <p className="text-sm text-muted-foreground">Monitor and manage event delivery pipeline</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search events..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Statuses</option>
          <option value="pending">Pending</option>
          <option value="processing">Processing</option>
          <option value="processed">Processed</option>
          <option value="failed">Failed</option>
          <option value="dead_lettered">Dead Lettered</option>
          <option value="skipped">Skipped</option>
          <option value="cancelled">Cancelled</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(e) => e.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} onRowClick={(e) => window.location.href = `/admin/events/outbox/${e.id}`} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={handleAction}
          confirmDisabled={(action.type === 'skip' || action.type === 'deadletter') && !reason}
          title={action.type === 'retry' ? 'Retry Event' : action.type === 'skip' ? 'Skip Event' : 'Dead-Letter Event'}
          description={
            <div>
              {action.type === 'retry' && <p className="text-sm">Retry processing this outbox event?</p>}
              {action.type === 'skip' && <p className="text-sm text-yellow-600">This event will be skipped and not delivered. This action requires a reason.</p>}
              {action.type === 'deadletter' && <p className="text-sm text-red-600">This event will be moved to dead-letter state. This action requires a reason.</p>}
              {(action.type === 'skip' || action.type === 'deadletter') && (
                <div className="mt-3">
                  <label className="text-xs font-medium">Reason *</label>
                  <input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="Enter reason..." />
                </div>
              )}
            </div>
          }
          confirmLabel={action.type === 'retry' ? 'Retry' : action.type === 'skip' ? 'Skip Event' : 'Dead-Letter'}
          destructive={action.type !== 'retry'} />
      )}
    </div>
  );
}
