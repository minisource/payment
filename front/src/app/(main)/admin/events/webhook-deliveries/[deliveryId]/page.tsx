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
import { getWebhookDelivery, retryWebhookDelivery, deadLetterWebhookDelivery } from '@/api/webhook-deliveries';
import { RefreshCw, XCircle } from 'lucide-react';

export default function WebhookDeliveryDetailPage() {
  return (
    <RoutePermissionGuard permissions={['payment.webhook.delivery.view_admin']}>
      <DeliveryDetailContent />
    </RoutePermissionGuard>
  );
}

function DeliveryDetailContent() {
  const { deliveryId } = useParams<{ deliveryId: string }>();
  const queryClient = useQueryClient();
  const [action, setAction] = useState<{ type: string } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['webhook-delivery', deliveryId],
    queryFn: () => getWebhookDelivery(deliveryId as string),
    enabled: !!deliveryId,
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'retry') await retryWebhookDelivery(deliveryId as string);
      else if (action.type === 'deadletter') await deadLetterWebhookDelivery(deliveryId as string, { reason: reason || 'Dead-lettered' });
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['webhook-delivery', deliveryId] });
      refetch();
      setReason('');
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !data) return <ErrorState error={(error as Error)?.message} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <PageHeader
        title="Webhook Delivery Detail"
        backHref="/admin/events/webhook-deliveries"
        actions={
          <div className="flex gap-2">
            {(data.status === 'failed' || data.status === 'dead_lettered') && (
              <PermissionGuard permission="payment.webhook.delivery.retry">
                <Button size="sm" variant="outline" onClick={() => setAction({ type: 'retry' })}><RefreshCw className="h-4 w-4 mr-1" />Retry</Button>
              </PermissionGuard>
            )}
            {data.status !== 'dead_lettered' && data.status !== 'cancelled' && (
              <PermissionGuard permission="payment.webhook.delivery.dead_letter">
                <Button size="sm" variant="outline" onClick={() => setAction({ type: 'deadletter' })} className="text-red-600"><XCircle className="h-4 w-4 mr-1" />Dead-Letter</Button>
              </PermissionGuard>
            )}
          </div>
        }
      />

      {/* Summary */}
      <DetailCard title="Delivery Summary">
        <div className="grid gap-3 sm:grid-cols-3">
          <div><span className="text-xs text-muted-foreground">Delivery ID</span><p className="font-mono text-sm">{data.id}</p></div>
          <div><span className="text-xs text-muted-foreground">Event Type</span><p className="font-mono text-sm">{data.eventType}</p></div>
          <div><span className="text-xs text-muted-foreground">Status</span><p><StatusBadge status={data.status} /></p></div>
          <div><span className="text-xs text-muted-foreground">HTTP Status</span><p className={`text-sm font-mono ${data.httpStatusCode && data.httpStatusCode >= 400 ? 'text-red-600' : 'text-green-600'}`}>{data.httpStatusCode || '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Attempts</span><p className="text-sm">{data.attemptCount} / {data.maxAttempts}</p></div>
          <div><span className="text-xs text-muted-foreground">Next Retry</span><p className="text-sm">{data.nextRetryAt ? new Date(data.nextRetryAt).toLocaleString() : '—'}</p></div>
        </div>
      </DetailCard>

      {/* Related entities */}
      <DetailCard title="Related Entities">
        <div className="grid gap-3 sm:grid-cols-2">
          <div><span className="text-xs text-muted-foreground">Subscription</span><p>{data.subscriptionName ? <Link href={`/admin/events/webhooks/${data.subscriptionId}`} className="text-primary hover:underline text-sm">{data.subscriptionName}</Link> : <span className="font-mono text-sm">{data.subscriptionId?.slice(0, 12)}…</span>}</p></div>
          <div><span className="text-xs text-muted-foreground">Outbox Event</span><p><Link href={`/admin/events/outbox/${data.outboxEventId}`} className="font-mono text-xs text-primary hover:underline">{data.outboxEventId?.slice(0, 12)}…</Link></p></div>
          {data.subscriptionTargetUrl && <div className="sm:col-span-2"><span className="text-xs text-muted-foreground">Target URL</span><p className="font-mono text-xs break-all">{data.subscriptionTargetUrl}</p></div>}
        </div>
      </DetailCard>

      {/* Timeline */}
      <DetailCard title="Timeline">
        <div className="grid gap-3 sm:grid-cols-3">
          <div><span className="text-xs text-muted-foreground">Created</span><p className="text-sm">{new Date(data.createdAt).toLocaleString()}</p></div>
          <div><span className="text-xs text-muted-foreground">Delivered</span><p className="text-sm">{data.deliveredAt ? new Date(data.deliveredAt).toLocaleString() : '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Failed At</span><p className="text-sm">{data.failedAt ? new Date(data.failedAt).toLocaleString() : '—'}</p></div>
        </div>
      </DetailCard>

      {/* Error */}
      {data.lastError && (
        <Card className="border-red-200 bg-red-50"><CardContent className="p-4"><p className="text-sm font-medium text-red-800">Error</p><pre className="mt-1 whitespace-pre-wrap text-xs text-red-700">{data.lastError}</pre></CardContent></Card>
      )}

      {/* Attempts */}
      {data.attempts && data.attempts.length > 0 && (
        <DetailCard title="Attempt History">
          <table className="w-full text-xs">
            <thead><tr className="border-b text-muted-foreground"><th className="p-2 text-left">#</th><th className="p-2 text-left">Started</th><th className="p-2 text-left">HTTP</th><th className="p-2 text-left">Duration</th><th className="p-2 text-left">Result</th><th className="p-2 text-left">Error</th></tr></thead>
            <tbody>{data.attempts.map((a) => (
              <tr key={a.attempt} className="border-b">
                <td className="p-2">{a.attempt}</td>
                <td className="p-2">{new Date(a.startedAt).toLocaleString()}</td>
                <td className="p-2 font-mono">{a.httpStatus || '—'}</td>
                <td className="p-2">{a.durationMs ? `${a.durationMs}ms` : '—'}</td>
                <td className="p-2">{a.success ? <span className="text-green-600">✓</span> : <span className="text-red-600">✗</span>}</td>
                <td className="p-2 text-red-600 max-w-[200px] truncate">{a.error || '—'}</td>
              </tr>
            ))}</tbody>
          </table>
        </DetailCard>
      )}

      {/* Request Payload (redacted) */}
      {data.requestPayload && (
        <DetailCard title="Request Payload (Redacted)">
          <p className="text-xs text-muted-foreground mb-2">Headers redacted for security. Signature/secret headers are hidden.</p>
          <SafePayloadViewer data={data.requestPayload} />
        </DetailCard>
      )}

      {/* Response */}
      {data.responseBody && (
        <DetailCard title="Response Body (Truncated)">
          <SafePayloadViewer data={typeof data.responseBody === 'string' ? { body: data.responseBody.substring(0, 2000) } : data.responseBody} />
        </DetailCard>
      )}

      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={handleAction}
          confirmDisabled={action.type === 'deadletter' && !reason}
          title={action.type === 'retry' ? 'Retry Delivery' : 'Dead-Letter Delivery'}
          description={
            <div>
              {action.type === 'retry' && <p className="text-sm">Retry this delivery?</p>}
              {action.type === 'deadletter' && <p className="text-sm text-red-600">Move to dead-letter. Requires reason.</p>}
              {action.type === 'deadletter' && <div className="mt-3"><label className="text-xs font-medium">Reason *</label><input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>}
            </div>
          }
          confirmLabel={action.type === 'retry' ? 'Retry' : 'Dead-Letter'} destructive={action.type !== 'retry'} />
      )}
    </div>
  );
}
