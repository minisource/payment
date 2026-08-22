'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { PageHeader } from '@/components/shared/page-header';
import { DetailCard } from '@/components/shared/detail-card';
import { toast } from 'sonner';
import { getEventRoutingRule, enableEventRoutingRule, disableEventRoutingRule, deleteEventRoutingRule } from '@/api/event-routing';
import { Power, PowerOff, Trash2 } from 'lucide-react';

const TARGET_TYPE_BADGE: Record<string, string> = {
  webhook: 'bg-blue-100 text-blue-700',
  notifier: 'bg-purple-100 text-purple-700',
  message_bus: 'bg-green-100 text-green-700',
  internal_handler: 'bg-gray-100 text-gray-700',
};

export default function EventRoutingRuleDetailPage() {
  return (
    <RoutePermissionGuard permissions={['payment.event_routing.view_admin']}>
      <RoutingRuleDetailContent />
    </RoutePermissionGuard>
  );
}

function RoutingRuleDetailContent() {
  const { routingRuleId } = useParams<{ routingRuleId: string }>();
  const queryClient = useQueryClient();
  const [action, setAction] = useState<{ type: string } | null>(null);

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['event-routing-rule', routingRuleId],
    queryFn: () => getEventRoutingRule(routingRuleId as string),
    enabled: !!routingRuleId,
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'enable') await enableEventRoutingRule(routingRuleId as string);
      else if (action.type === 'disable') await disableEventRoutingRule(routingRuleId as string);
      else if (action.type === 'delete') { await deleteEventRoutingRule(routingRuleId as string); window.location.href = '/admin/events/routing'; return; }
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['event-routing-rule', routingRuleId] });
      refetch();
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !data) return <ErrorState error={(error as Error)?.message} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <PageHeader
        title={data.name}
        backHref="/admin/events/routing"
        actions={
          <div className="flex gap-2">
            <PermissionGuard permission="payment.event_routing.update">
              <Link href={`/admin/events/routing/${routingRuleId}/edit`}><Button size="sm" variant="outline">Edit</Button></Link>
            </PermissionGuard>
            {data.status !== 'active' && (
              <PermissionGuard permission="payment.event_routing.update">
                <Button size="sm" variant="outline" onClick={() => setAction({ type: 'enable' })} className="text-green-600"><Power className="h-4 w-4 mr-1" />Enable</Button>
              </PermissionGuard>
            )}
            {data.status === 'active' && (
              <PermissionGuard permission="payment.event_routing.update">
                <Button size="sm" variant="outline" onClick={() => setAction({ type: 'disable' })} className="text-yellow-600"><PowerOff className="h-4 w-4 mr-1" />Disable</Button>
              </PermissionGuard>
            )}
            <PermissionGuard permission="payment.event_routing.delete">
              <Button size="sm" variant="outline" onClick={() => setAction({ type: 'delete' })} className="text-red-600"><Trash2 className="h-4 w-4 mr-1" />Delete</Button>
            </PermissionGuard>
          </div>
        }
      />

      <DetailCard title="Rule Summary">
        <div className="grid gap-3 sm:grid-cols-3">
          <div><span className="text-xs text-muted-foreground">ID</span><p className="font-mono text-sm">{data.id}</p></div>
          <div><span className="text-xs text-muted-foreground">Name</span><p className="text-sm font-medium">{data.name}</p></div>
          <div><span className="text-xs text-muted-foreground">Status</span><p><StatusBadge status={data.status} /></p></div>
        </div>
        {data.description && <div className="mt-3"><span className="text-xs text-muted-foreground">Description</span><p className="text-sm">{data.description}</p></div>}
      </DetailCard>

      <DetailCard title="Routing Configuration">
        <div className="grid gap-3 sm:grid-cols-2">
          <div><span className="text-xs text-muted-foreground">Target Type</span><p><span className={`rounded px-2 py-0.5 text-xs font-medium ${TARGET_TYPE_BADGE[data.targetType] || ''}`}>{data.targetType.replace(/_/g, ' ')}</span></p></div>
          <div><span className="text-xs text-muted-foreground">Priority</span><p className="text-sm">{data.priority}</p></div>
          <div className="sm:col-span-2"><span className="text-xs text-muted-foreground">Event Types</span><p className="text-sm">{data.eventTypes.join(', ')}</p></div>
        </div>
      </DetailCard>

      <DetailCard title="Scope">
        <div className="grid gap-3 sm:grid-cols-2">
          <div><span className="text-xs text-muted-foreground">Tenant</span><p className="font-mono text-sm">{data.tenantId}</p></div>
          <div><span className="text-xs text-muted-foreground">Application</span><p className="text-sm">{data.applicationCode || '— (global)'}</p></div>
        </div>
      </DetailCard>

      {data.targetConfig && Object.keys(data.targetConfig).length > 0 && (
        <DetailCard title="Target Config">
          <SafePayloadViewer data={data.targetConfig} />
        </DetailCard>
      )}

      {data.conditions && Object.keys(data.conditions).length > 0 && (
        <DetailCard title="Conditions">
          <SafePayloadViewer data={data.conditions} />
        </DetailCard>
      )}

      <DetailCard title="Timestamps">
        <div className="grid gap-3 sm:grid-cols-2">
          <div><span className="text-xs text-muted-foreground">Created</span><p className="text-sm">{new Date(data.createdAt).toLocaleString()}</p></div>
          <div><span className="text-xs text-muted-foreground">Updated</span><p className="text-sm">{new Date(data.updatedAt).toLocaleString()}</p></div>
        </div>
      </DetailCard>

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
