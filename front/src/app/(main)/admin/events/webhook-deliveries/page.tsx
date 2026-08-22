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
import { listWebhookDeliveries, retryWebhookDelivery, deadLetterWebhookDelivery, type WebhookDeliveryListItem } from '@/api/webhook-deliveries';
import { RefreshCw, XCircle } from 'lucide-react';

const PAGE_SIZE = 20;

export default function WebhookDeliveriesPage() {
  return (
    <RoutePermissionGuard permissions={['payment.webhook.delivery.view_admin']}>
      <DeliveriesContent />
    </RoutePermissionGuard>
  );
}

function DeliveriesContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['webhook-deliveries', { skip, search, status: statusFilter }],
    queryFn: () => listWebhookDeliveries({ skip, take: PAGE_SIZE, query: search || undefined, status: statusFilter || undefined }),
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'retry') await retryWebhookDelivery(action.id);
      else if (action.type === 'deadletter') await deadLetterWebhookDelivery(action.id, { reason: reason || 'Dead-lettered' });
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['webhook-deliveries'] });
      setReason('');
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  const columns: DataTableColumn<WebhookDeliveryListItem>[] = [
    { key: 'id', header: 'Delivery ID', render: (d) => <Link href={`/admin/events/webhook-deliveries/${d.id}`} className="font-mono text-xs text-primary hover:underline">{d.id.slice(0, 12)}…</Link> },
    { key: 'eventType', header: 'Event Type', render: (d) => <span className="text-xs font-mono">{d.eventType}</span> },
    { key: 'status', header: 'Status', render: (d) => <StatusBadge status={d.status} /> },
    { key: 'httpCode', header: 'HTTP', render: (d) => <span className={`text-xs font-mono ${d.httpStatusCode && d.httpStatusCode >= 400 ? 'text-red-600' : d.httpStatusCode ? 'text-green-600' : ''}`}>{d.httpStatusCode || '—'}</span> },
    { key: 'attempts', header: 'Attempts', render: (d) => <span className="text-xs">{d.attemptCount}</span> },
    { key: 'error', header: 'Error', render: (d) => <span className="text-xs text-red-600 max-w-[150px] truncate block">{d.lastError || '—'}</span> },
    { key: 'created', header: 'Created', render: (d) => <span className="text-xs">{new Date(d.createdAt).toLocaleString()}</span> },
    { key: 'delivered', header: 'Delivered', render: (d) => <span className="text-xs">{d.deliveredAt ? new Date(d.deliveredAt).toLocaleString() : '—'}</span> },
    { key: 'actions', header: '', render: (d) => (
      <div className="flex gap-1">
        {(d.status === 'failed' || d.status === 'dead_lettered') && (
          <PermissionGuard permission="payment.webhook.delivery.retry">
            <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: d.id, type: 'retry' }); }} className="text-xs text-green-600" title="Retry"><RefreshCw className="h-3 w-3" /></Button>
          </PermissionGuard>
        )}
        {d.status !== 'dead_lettered' && d.status !== 'cancelled' && (
          <PermissionGuard permission="payment.webhook.delivery.dead_letter">
            <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: d.id, type: 'deadletter' }); }} className="text-xs text-red-600" title="Dead-letter"><XCircle className="h-3 w-3" /></Button>
          </PermissionGuard>
        )}
      </div>
    ) },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Webhook Deliveries</h1>
          <p className="text-sm text-muted-foreground">Delivery attempts, responses, and retries</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option><option value="pending">Pending</option><option value="processing">Processing</option><option value="delivered">Delivered</option><option value="failed">Failed</option><option value="dead_lettered">Dead Lettered</option><option value="skipped">Skipped</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(d) => d.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} onRowClick={(d) => window.location.href = `/admin/events/webhook-deliveries/${d.id}`} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={handleAction}
          confirmDisabled={action.type === 'deadletter' && !reason}
          title={action.type === 'retry' ? 'Retry Delivery' : 'Dead-Letter Delivery'}
          description={
            <div>
              {action.type === 'retry' && <p className="text-sm">Retry this webhook delivery?</p>}
              {action.type === 'deadletter' && <p className="text-sm text-red-600">Move this delivery to dead-letter state. Requires a reason.</p>}
              {action.type === 'deadletter' && (
                <div className="mt-3"><label className="text-xs font-medium">Reason *</label><input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>
              )}
            </div>
          }
          confirmLabel={action.type === 'retry' ? 'Retry' : 'Dead-Letter'} destructive={action.type !== 'retry'} />
      )}
    </div>
  );
}
