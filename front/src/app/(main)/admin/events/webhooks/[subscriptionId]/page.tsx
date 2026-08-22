'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { PageHeader } from '@/components/shared/page-header';
import { DetailCard } from '@/components/shared/detail-card';
import { toast } from 'sonner';
import { getWebhookSubscription, enableWebhookSubscription, disableWebhookSubscription, deleteWebhookSubscription, regenerateWebhookSecret, type WebhookSecretRegenerateResponse } from '@/api/webhooks';
import { ArrowLeft, RotateCw, Power, PowerOff, Trash2, Copy, Shield } from 'lucide-react';

export default function WebhookDetailPage() {
  return (
    <RoutePermissionGuard permissions={['payment.webhook.view_admin']}>
      <WebhookDetailContent />
    </RoutePermissionGuard>
  );
}

function WebhookDetailContent() {
  const { subscriptionId } = useParams<{ subscriptionId: string }>();
  const queryClient = useQueryClient();
  const [action, setAction] = useState<{ type: string } | null>(null);
  const [newSecret, setNewSecret] = useState<WebhookSecretRegenerateResponse | null>(null);

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['webhook', subscriptionId],
    queryFn: () => getWebhookSubscription(subscriptionId as string),
    enabled: !!subscriptionId,
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'enable') await enableWebhookSubscription(subscriptionId as string);
      else if (action.type === 'disable') await disableWebhookSubscription(subscriptionId as string);
      else if (action.type === 'delete') { await deleteWebhookSubscription(subscriptionId as string); window.location.href = '/admin/events/webhooks'; return; }
      else if (action.type === 'regenerate') {
        const res = await regenerateWebhookSecret(subscriptionId as string);
        setNewSecret(res);
        setAction(null);
        queryClient.invalidateQueries({ queryKey: ['webhook', subscriptionId] });
        return;
      }
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['webhook', subscriptionId] });
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
        backHref="/admin/events/webhooks"
        actions={
          <div className="flex gap-2">
            <PermissionGuard permission="payment.webhook.update">
              <Link href={`/admin/events/webhooks/${subscriptionId}/edit`}><Button size="sm" variant="outline">Edit</Button></Link>
            </PermissionGuard>
            {data.status !== 'active' && (
              <PermissionGuard permission="payment.webhook.update">
                <Button size="sm" variant="outline" onClick={() => setAction({ type: 'enable' })} className="text-green-600"><Power className="h-4 w-4 mr-1" />Enable</Button>
              </PermissionGuard>
            )}
            {data.status === 'active' && (
              <PermissionGuard permission="payment.webhook.update">
                <Button size="sm" variant="outline" onClick={() => setAction({ type: 'disable' })} className="text-yellow-600"><PowerOff className="h-4 w-4 mr-1" />Disable</Button>
              </PermissionGuard>
            )}
            <PermissionGuard permission="payment.webhook.secret.regenerate">
              <Button size="sm" variant="outline" onClick={() => setAction({ type: 'regenerate' })} className="text-orange-600"><RotateCw className="h-4 w-4 mr-1" />Regenerate Secret</Button>
            </PermissionGuard>
            <PermissionGuard permission="payment.webhook.delete">
              <Button size="sm" variant="outline" onClick={() => setAction({ type: 'delete' })} className="text-red-600"><Trash2 className="h-4 w-4 mr-1" />Delete</Button>
            </PermissionGuard>
          </div>
        }
      />

      {/* Summary */}
      <DetailCard title="Subscription Summary">
        <div className="grid gap-3 sm:grid-cols-3">
          <div><span className="text-xs text-muted-foreground">ID</span><p className="font-mono text-sm">{data.id}</p></div>
          <div><span className="text-xs text-muted-foreground">Name</span><p className="text-sm font-medium">{data.name}</p></div>
          <div><span className="text-xs text-muted-foreground">Status</span><p><StatusBadge status={data.status} /></p></div>
        </div>
        {data.description && <div className="mt-3"><span className="text-xs text-muted-foreground">Description</span><p className="text-sm">{data.description}</p></div>}
      </DetailCard>

      {/* Target */}
      <DetailCard title="Target Configuration">
        <div className="grid gap-3 sm:grid-cols-2">
          <div className="sm:col-span-2"><span className="text-xs text-muted-foreground">Target URL</span><p className="font-mono text-sm break-all">{data.targetUrl}</p></div>
          <div><span className="text-xs text-muted-foreground">Event Types</span><p className="text-sm">{data.eventTypes.join(', ')}</p></div>
          <div><span className="text-xs text-muted-foreground">Secret</span><p className="text-sm">{data.secretConfigured ? <span className="text-green-600 font-medium">Configured</span> : <span className="text-red-600">Not configured</span>}</p></div>
        </div>
      </DetailCard>

      {/* Tenant/App */}
      <DetailCard title="Scope">
        <div className="grid gap-3 sm:grid-cols-2">
          <div><span className="text-xs text-muted-foreground">Tenant</span><p className="font-mono text-sm">{data.tenantId}</p></div>
          <div><span className="text-xs text-muted-foreground">Application</span><p className="text-sm">{data.applicationCode || '—'} <span className="text-xs text-muted-foreground">(null = global)</span></p></div>
        </div>
      </DetailCard>

      {/* Health */}
      <DetailCard title="Delivery Health">
        <div className="grid gap-3 sm:grid-cols-3">
          <div><span className="text-xs text-muted-foreground">Last Delivery</span><p className="text-sm">{data.lastDeliveryAt ? new Date(data.lastDeliveryAt).toLocaleString() : '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Last Success</span><p className="text-sm">{data.lastSuccessAt ? new Date(data.lastSuccessAt).toLocaleString() : '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Last Failure</span><p className="text-sm">{data.lastFailureAt ? new Date(data.lastFailureAt).toLocaleString() : '—'}</p></div>
        </div>
      </DetailCard>

      {/* Dates */}
      <DetailCard title="Timestamps">
        <div className="grid gap-3 sm:grid-cols-2">
          <div><span className="text-xs text-muted-foreground">Created</span><p className="text-sm">{new Date(data.createdAt).toLocaleString()}</p></div>
          <div><span className="text-xs text-muted-foreground">Updated</span><p className="text-sm">{new Date(data.updatedAt).toLocaleString()}</p></div>
        </div>
      </DetailCard>

      {/* Regular confirm dialogs */}
      {action && action.type !== 'regenerate' && (
        <ConfirmDialog open onClose={() => setAction(null)} onConfirm={handleAction}
          title={action.type === 'enable' ? 'Enable Webhook' : action.type === 'disable' ? 'Disable Webhook' : 'Delete Webhook'}
          description={action.type === 'delete' ? 'This will permanently delete the webhook subscription. All pending deliveries will be cancelled.' : `${action.type === 'enable' ? 'Enable' : 'Disable'} this webhook subscription?`}
          confirmLabel={action.type === 'enable' ? 'Enable' : action.type === 'disable' ? 'Disable' : 'Delete'}
          destructive={action.type === 'delete'} />
      )}

      {/* Regenerate secret confirm dialog */}
      {action && action.type === 'regenerate' && (
        <ConfirmDialog open onClose={() => setAction(null)} onConfirm={handleAction}
          title="Regenerate Webhook Secret"
          description={<p className="text-sm text-orange-600">This will generate a new secret. The old secret may stop working depending on backend behavior. The new secret will only be shown once.</p>}
          confirmLabel="Regenerate"
          destructive />
      )}

      {/* One-time secret modal */}
      {newSecret && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="w-full max-w-md rounded-lg bg-background p-6 shadow-lg">
            <div className="flex items-center gap-2 mb-4"><Shield className="h-5 w-5 text-orange-600" /><h2 className="text-lg font-bold">New Webhook Secret</h2></div>
            <p className="text-sm text-orange-600 mb-4">This secret will not be shown again. Copy and store it securely now.</p>
            <div className="flex items-center gap-2 rounded-md border bg-muted p-3">
              <code className="flex-1 break-all text-xs font-mono">{newSecret.secret}</code>
              <Button size="sm" variant="ghost" onClick={() => { navigator.clipboard.writeText(newSecret.secret); toast.success('Secret copied'); }}><Copy className="h-4 w-4" /></Button>
            </div>
            <p className="text-xs text-muted-foreground mt-2">Rotated at: {new Date(newSecret.rotatedAt).toLocaleString()}</p>
            <Button className="mt-4 w-full" onClick={() => setNewSecret(null)}>I've saved the secret</Button>
          </div>
        </div>
      )}
    </div>
  );
}
