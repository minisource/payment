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
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { PageHeader } from '@/components/shared/page-header';
import { DetailCard } from '@/components/shared/detail-card';
import { toast } from 'sonner';
import { getOutboxEvent, retryOutboxEvent, skipOutboxEvent, deadLetterOutboxEvent } from '@/api/outbox';
import { ArrowLeft, RefreshCw, SkipForward, XCircle } from 'lucide-react';

export default function OutboxEventDetailPage() {
  return (
    <RoutePermissionGuard permissions={['payment.outbox.view_admin']}>
      <OutboxEventDetailContent />
    </RoutePermissionGuard>
  );
}

function OutboxEventDetailContent() {
  const { eventId } = useParams<{ eventId: string }>();
  const queryClient = useQueryClient();
  const [action, setAction] = useState<{ type: string } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['outbox-event', eventId],
    queryFn: () => getOutboxEvent(eventId as string),
    enabled: !!eventId,
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'retry') await retryOutboxEvent(eventId as string);
      else if (action.type === 'skip') await skipOutboxEvent(eventId as string, { reason: reason || 'Manually skipped' });
      else if (action.type === 'deadletter') await deadLetterOutboxEvent(eventId as string, { reason: reason || 'Manually dead-lettered' });
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['outbox-event', eventId] });
      queryClient.invalidateQueries({ queryKey: ['outbox'] });
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
        title="Outbox Event Detail"
        backHref="/admin/events/outbox"
        actions={
          <div className="flex gap-2">
            {(data.status === 'failed' || data.status === 'dead_lettered') && (
              <PermissionGuard permission="payment.outbox.retry">
                <Button size="sm" variant="outline" onClick={() => setAction({ type: 'retry' })}><RefreshCw className="h-4 w-4 mr-1" />Retry</Button>
              </PermissionGuard>
            )}
            {data.status !== 'processed' && data.status !== 'cancelled' && (
              <PermissionGuard permission="payment.outbox.skip">
                <Button size="sm" variant="outline" onClick={() => setAction({ type: 'skip' })} className="text-yellow-600"><SkipForward className="h-4 w-4 mr-1" />Skip</Button>
              </PermissionGuard>
            )}
            {data.status !== 'dead_lettered' && data.status !== 'cancelled' && (
              <PermissionGuard permission="payment.outbox.dead_letter">
                <Button size="sm" variant="outline" onClick={() => setAction({ type: 'deadletter' })} className="text-red-600"><XCircle className="h-4 w-4 mr-1" />Dead-Letter</Button>
              </PermissionGuard>
            )}
          </div>
        }
      />

      {/* Summary */}
      <DetailCard title="Event Summary">
        <div className="grid gap-3 sm:grid-cols-3">
          <div><span className="text-xs text-muted-foreground">Event ID</span><p className="font-mono text-sm">{data.id}</p></div>
          <div><span className="text-xs text-muted-foreground">Event Type</span><p className="font-mono text-sm">{data.eventType}</p></div>
          <div><span className="text-xs text-muted-foreground">Status</span><p><StatusBadge status={data.status} /></p></div>
          <div><span className="text-xs text-muted-foreground">Aggregate</span><p className="font-mono text-sm">{data.aggregateType}/{data.aggregateId?.slice(0, 12)}…</p></div>
          <div><span className="text-xs text-muted-foreground">Attempts</span><p className="text-sm">{data.attemptCount} / {data.maxAttempts}</p></div>
          <div><span className="text-xs text-muted-foreground">Payload Version</span><p className="text-sm">v{data.payloadVersion}</p></div>
        </div>
      </DetailCard>

      {/* Timeline */}
      <DetailCard title="Timeline">
        <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
          <div><span className="text-xs text-muted-foreground">Created</span><p className="text-sm">{new Date(data.createdAt).toLocaleString()}</p></div>
          <div><span className="text-xs text-muted-foreground">Available At</span><p className="text-sm">{data.availableAt ? new Date(data.availableAt).toLocaleString() : '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Next Retry</span><p className="text-sm">{data.nextRetryAt ? new Date(data.nextRetryAt).toLocaleString() : '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Processed At</span><p className="text-sm">{data.processedAt ? new Date(data.processedAt).toLocaleString() : '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Failed At</span><p className="text-sm">{data.failedAt ? new Date(data.failedAt).toLocaleString() : '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Dead-Lettered At</span><p className="text-sm">{data.deadLetteredAt ? new Date(data.deadLetteredAt).toLocaleString() : '—'}</p></div>
        </div>
      </DetailCard>

      {/* IDs */}
      <DetailCard title="Request Info">
        <div className="grid gap-3 sm:grid-cols-2">
          <div><span className="text-xs text-muted-foreground">Request ID</span><p className="font-mono text-sm">{data.requestId || '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Correlation ID</span><p className="font-mono text-sm">{data.correlationId || '—'}</p></div>
        </div>
      </DetailCard>

      {/* Payload */}
      {data.payload && (
        <DetailCard title="Event Payload">
          <SafePayloadViewer data={data.payload} />
        </DetailCard>
      )}

      {/* Delivery Attempts */}
      {data.deliveryAttempts && data.deliveryAttempts.length > 0 && (
        <DetailCard title="Delivery Attempts">
          <table className="w-full text-xs">
            <thead><tr className="border-b text-muted-foreground"><th className="p-2 text-left">#</th><th className="p-2 text-left">Started</th><th className="p-2 text-left">Completed</th><th className="p-2 text-left">Result</th><th className="p-2 text-left">Error</th></tr></thead>
            <tbody>{data.deliveryAttempts.map((a) => (
              <tr key={a.attempt} className="border-b">
                <td className="p-2">{a.attempt}</td>
                <td className="p-2">{new Date(a.startedAt).toLocaleString()}</td>
                <td className="p-2">{a.completedAt ? new Date(a.completedAt).toLocaleString() : '—'}</td>
                <td className="p-2">{a.success ? <span className="text-green-600">✓</span> : <span className="text-red-600">✗</span>}</td>
                <td className="p-2 text-red-600 max-w-[200px] truncate">{a.error || '—'}</td>
              </tr>
            ))}</tbody>
          </table>
        </DetailCard>
      )}

      {/* Error */}
      {data.errorMessage && (
        <Card className="border-red-200 bg-red-50">
          <CardContent className="p-4">
            <p className="text-sm font-medium text-red-800">Error</p>
            <pre className="mt-1 whitespace-pre-wrap text-xs text-red-700">{data.errorMessage}</pre>
          </CardContent>
        </Card>
      )}

      {/* Action dialogs */}
      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={handleAction}
          confirmDisabled={(action.type === 'skip' || action.type === 'deadletter') && !reason}
          title={action.type === 'retry' ? 'Retry Event' : action.type === 'skip' ? 'Skip Event' : 'Dead-Letter Event'}
          description={
            <div>
              {action.type === 'retry' && <p className="text-sm">Retry processing this event?</p>}
              {action.type === 'skip' && <p className="text-sm text-yellow-600">This event will be skipped and not delivered. Requires a reason.</p>}
              {action.type === 'deadletter' && <p className="text-sm text-red-600">This event will be moved to dead-letter state. Requires a reason.</p>}
              {(action.type === 'skip' || action.type === 'deadletter') && (
                <div className="mt-3"><label className="text-xs font-medium">Reason *</label><input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>
              )}
            </div>
          }
          confirmLabel={action.type === 'retry' ? 'Retry' : action.type === 'skip' ? 'Skip' : 'Dead-Letter'}
          destructive={action.type !== 'retry'} />
      )}
    </div>
  );
}
