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
import { listWebhookSubscriptions, enableWebhookSubscription, disableWebhookSubscription, deleteWebhookSubscription, type WebhookSubscriptionListItem } from '@/api/webhooks';
import { Plus, Power, PowerOff, Trash2, Globe } from 'lucide-react';

const PAGE_SIZE = 20;

export default function WebhooksPage() {
  return (
    <RoutePermissionGuard permissions={['payment.webhook.view_admin']}>
      <WebhooksContent />
    </RoutePermissionGuard>
  );
}

function WebhooksContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string } | null>(null);

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['webhooks', { skip, search, status: statusFilter }],
    queryFn: () => listWebhookSubscriptions({ skip, take: PAGE_SIZE, query: search || undefined, status: statusFilter || undefined }),
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'enable') await enableWebhookSubscription(action.id);
      else if (action.type === 'disable') await disableWebhookSubscription(action.id);
      else if (action.type === 'delete') await deleteWebhookSubscription(action.id);
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['webhooks'] });
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  const columns: DataTableColumn<WebhookSubscriptionListItem>[] = [
    { key: 'name', header: 'Name', render: (s) => <Link href={`/admin/events/webhooks/${s.id}`} className="text-primary hover:underline font-medium text-sm">{s.name}</Link> },
    { key: 'targetUrl', header: 'Target URL', render: (s) => <span className="font-mono text-xs max-w-[200px] truncate block">{s.targetUrl}</span> },
    { key: 'status', header: 'Status', render: (s) => <StatusBadge status={s.status} /> },
    { key: 'eventTypes', header: 'Events', render: (s) => <span className="text-xs">{s.eventTypes.slice(0, 3).join(', ')}{s.eventTypes.length > 3 ? ` +${s.eventTypes.length - 3}` : ''}</span> },
    { key: 'lastDelivery', header: 'Last Delivery', render: (s) => <span className="text-xs">{s.lastDeliveryAt ? new Date(s.lastDeliveryAt).toLocaleString() : '—'}</span> },
    { key: 'created', header: 'Created', render: (s) => <span className="text-xs">{new Date(s.createdAt).toLocaleDateString()}</span> },
    { key: 'actions', header: '', render: (s) => (
      <div className="flex gap-1">
        {s.status !== 'active' && (
          <PermissionGuard permission="payment.webhook.update">
            <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: s.id, type: 'enable' }); }} className="text-xs text-green-600" title="Enable"><Power className="h-3 w-3" /></Button>
          </PermissionGuard>
        )}
        {s.status === 'active' && (
          <PermissionGuard permission="payment.webhook.update">
            <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: s.id, type: 'disable' }); }} className="text-xs text-yellow-600" title="Disable"><PowerOff className="h-3 w-3" /></Button>
          </PermissionGuard>
        )}
        <PermissionGuard permission="payment.webhook.delete">
          <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: s.id, type: 'delete' }); }} className="text-xs text-red-600" title="Delete"><Trash2 className="h-3 w-3" /></Button>
        </PermissionGuard>
      </div>
    ) },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Webhooks</h1>
          <p className="text-sm text-muted-foreground">Webhook subscriptions and delivery monitoring</p>
        </div>
        <div className="flex gap-2">
          <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
          <PermissionGuard permission="payment.webhook.create">
            <Link href="/admin/events/webhooks/new"><Button size="sm"><Plus className="h-4 w-4 mr-1" />New Webhook</Button></Link>
          </PermissionGuard>
        </div>
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option><option value="active">Active</option><option value="disabled">Disabled</option><option value="failing">Failing</option><option value="paused">Paused</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(s) => s.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} onRowClick={(s) => window.location.href = `/admin/events/webhooks/${s.id}`} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {action && (
        <ConfirmDialog open onClose={() => setAction(null)} onConfirm={handleAction}
          title={action.type === 'enable' ? 'Enable Webhook' : action.type === 'disable' ? 'Disable Webhook' : 'Delete Webhook'}
          description={action.type === 'delete' ? 'This will permanently delete the webhook subscription. All pending deliveries will be cancelled.' : `${action.type === 'enable' ? 'Enable' : 'Disable'} this webhook subscription?`}
          confirmLabel={action.type === 'enable' ? 'Enable' : action.type === 'disable' ? 'Disable' : 'Delete'}
          destructive={action.type === 'delete'} />
      )}
    </div>
  );
}
