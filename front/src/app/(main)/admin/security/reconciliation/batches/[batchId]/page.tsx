'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { getReconciliationBatch } from '@/api/reconciliation';

export default function ReconciliationBatchDetailPage() {
  const { batchId } = useParams<{ batchId: string }>();

  const { data: batch, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['rec-batch', batchId],
    queryFn: () => getReconciliationBatch(batchId),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !batch) return <ErrorState error={(error as Error)?.message || 'Not found'} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <div>
        <Link href="/admin/security/reconciliation/batches" className="text-sm text-muted-foreground hover:text-foreground">← Batches</Link>
        <div className="mt-1 flex items-center gap-3"><h1 className="text-xl font-bold">Batch {batch.id.slice(0, 12)}…</h1><StatusBadge status={batch.status} /></div>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Type</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm font-medium capitalize">{batch.batchType.replace(/_/g, ' ')}</p></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Status</CardTitle></CardHeader><CardContent className="p-3 pt-0"><StatusBadge status={batch.status} /></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Total Items</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm font-bold">{batch.totalItems}</p></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Created By</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-xs font-mono">{batch.createdBy?.slice(0, 12)}…</p></CardContent></Card>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <Card><CardContent className="p-4 text-center"><p className="text-2xl font-bold text-green-600">{batch.matchedCount}</p><p className="text-xs text-muted-foreground">Matched</p></CardContent></Card>
        <Card><CardContent className="p-4 text-center"><p className="text-2xl font-bold text-red-600">{batch.mismatchedCount}</p><p className="text-xs text-muted-foreground">Mismatched</p></CardContent></Card>
        <Card><CardContent className="p-4 text-center"><p className="text-2xl font-bold text-yellow-600">{batch.unresolvedCount}</p><p className="text-xs text-muted-foreground">Unresolved</p></CardContent></Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Timeline</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span>{new Date(batch.createdAt).toLocaleString()}</span></div>
            {batch.startedAt && <div className="flex justify-between"><span className="text-muted-foreground">Started</span><span>{new Date(batch.startedAt).toLocaleString()}</span></div>}
            {batch.completedAt && <div className="flex justify-between"><span className="text-muted-foreground">Completed</span><span>{new Date(batch.completedAt).toLocaleString()}</span></div>}
          </CardContent>
        </Card>
      </div>

      {batch.runParameters && <SafePayloadViewer data={batch.runParameters} title="Run Parameters" />}
      {batch.metadata && <SafePayloadViewer data={batch.metadata} title="Metadata" />}
      {batch.errorDetails && <Card className="border-red-200"><CardContent className="p-4"><p className="text-sm text-red-600 font-medium">Error</p><p className="mt-1 text-xs text-red-500">{batch.errorDetails}</p></CardContent></Card>}
    </div>
  );
}
