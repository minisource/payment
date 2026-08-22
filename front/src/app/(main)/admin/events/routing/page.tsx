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
import { listEventRoutingRules, enableEventRoutingRule, disableEventRoutingRule, deleteEventRoutingRule, type EventRoutingRuleListItem } from '@/api/event-routing';
import { Plus, Power, PowerOff, Trash2 } from 'lucide-react';

const PAGE_SIZE = 20;

const TARGET_TYPE_BADGE: Record<string, string> = {
  webhook: 'bg-blue-100 text-blue-700',
  notifier: 'bg-purple-100 text-purple-700',
  message_bus: 'bg-green-100 text-green-700',
  internal_handler: 'bg-gray-100 text-gray-700',
};

export default function EventRoutingPage() {
  return (
    <RoutePermissionGuard permissions={['payment.event_routing.view_admin']}>
      <RoutingContent />
    </RoutePermissionGuard>
  );
}

function RoutingContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string } | null>(null);

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['event-routing', { skip, search, status: statusFilter }],
    queryFn: () => listEventRoutingRules({ skip, take: PAGE_SIZE, query: search || undefined, status: statusFilter || undefined }),
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'enable') await enableEventRoutingRule(action.id);
      else if (action.type === 'disable') await disableEventRoutingRule(action.id);
      else if (action.type === 'delete') await deleteEventRoutingRule(action.id);
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['event-routing'] });
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  const columns: DataTableColumn<EventRoutingRuleListItem>[] = [
    { key: 'name', header: 'Name', render: (r) => <Link href={`/admin/events/routing/${r.id}`} className="text-primary hover:underline font-medium text-sm">{r.name}</Link> },
    { key: 'eventTypes', header: 'Event Types', render: (r) => <span className="text-xs">{r.eventTypes.slice(0, 3).join(', ')}{r.eventTypes.length > 3 ? ` +${r.eventTypes.length - 3}` : ''}</span> },
    { key: 'targetType', header: 'Target', render: (r) => <span className={`rounded px-2 py-0.5 text-xs font-medium ${TARGET_TYPE_BADGE[r.targetType] || ''}`}>{r.targetType.replace(/_/g, ' ')}</span> },
    { key: 'priority', header: 'Priority', render: (r) => <span className="text-xs">{r.priority}</span> },
    { key: 'status', header: 'Status', render: (r) => <StatusBadge status={r.status} /> },
    { key: 'created', header: 'Created', render: (r) => <span className="text-xs">{new Date(r.createdAt).toLocaleDateString()}</span> },
    { key: 'actions', header: '', render: (r) => (
      <div className="flex gap-1">
        {r.status !== 'active' && (
          <PermissionGuard permission="payment.event_routing.update">
            <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: r.id, type: 'enable' }); }} className="text-xs text-green-600" title="Enable"><Power className="h-3 w-3" /></Button>
          </PermissionGuard>
        )}
        {r.status === 'active' && (
          <PermissionGuard permission="payment.event_routing.update">
            <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: r.id, type: 'disable' }); }} className="text-xs text-yellow-600" title="Disable"><PowerOff className="h-3 w-3" /></Button>
          </PermissionGuard>
        )}
        <PermissionGuard permission="payment.event_routing.delete">
          <Button variant="ghost" size="sm" onClick={(ev) => { ev.stopPropagation(); setAction({ id: r.id, type: 'delete' }); }} className="text-xs text-red-600" title="Delete"><Trash2 className="h-3 w-3" /></Button>
        </PermissionGuard>
      </div>
    ) },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Event Routing Rules</h1>
          <p className="text-sm text-muted-foreground">Configure event routing targets</p>
        </div>
        <div className="flex gap-2">
          <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
          <PermissionGuard permission="payment.event_routing.create">
            <Link href="/admin/events/routing/new"><Button size="sm"><Plus className="h-4 w-4 mr-1" />New Rule</Button></Link>
          </PermissionGuard>
        </div>
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option><option value="active">Active</option><option value="disabled">Disabled</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(r) => r.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} onRowClick={(r) => window.location.href = `/admin/events/routing/${r.id}`} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {action && (
        <ConfirmDialog open onClose={() => setAction(null)} onConfirm={handleAction}
          title={action.type === 'enable' ? 'Enable Rule' : action.type === 'disable' ? 'Disable Rule' : 'Delete Rule'}
          description={action.type === 'delete' ? 'Permanently delete this routing rule?' : `${action.type === 'enable' ? 'Enable' : 'Disable'} this routing rule?`}
          confirmLabel={action.type === 'enable' ? 'Enable' : action.type === 'disable' ? 'Disable' : 'Delete'}
          destructive={action.type === 'delete'} />
      )}
    </div>
  );
}
