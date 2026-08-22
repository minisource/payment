'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { getReconciliationItem } from '@/api/reconciliation';

export default function ReconciliationItemDetailPage() {
  const { itemId } = useParams<{ itemId: string }>();

  const { data: item, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['rec-item', itemId],
    queryFn: () => getReconciliationItem(itemId),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !item) return <ErrorState error={(error as Error)?.message || 'Not found'} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <div>
        <Link href="/admin/security/reconciliation/items" className="text-sm text-muted-foreground hover:text-foreground">← Items</Link>
        <div className="mt-1 flex items-center gap-3"><h1 className="text-xl font-bold">Item {item.id.slice(0, 12)}…</h1><StatusBadge status={item.status} /></div>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Type</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm font-medium capitalize">{item.itemType.replace(/_/g, ' ')}</p></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Status</CardTitle></CardHeader><CardContent className="p-3 pt-0"><StatusBadge status={item.status} /></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Severity</CardTitle></CardHeader><CardContent className="p-3 pt-0"><StatusBadge status={item.severity} /></CardContent></Card>
        {item.batchId && <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Batch</CardTitle></CardHeader><CardContent className="p-3 pt-0"><Link href={`/admin/security/reconciliation/batches/${item.batchId}`} className="font-mono text-xs text-primary hover:underline">{item.batchId.slice(0, 12)}…</Link></CardContent></Card>}
      </div>

      <Card>
        <CardHeader><CardTitle className="text-sm">Amount Comparison</CardTitle></CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-3">
          <div className="text-center"><p className="text-xs text-muted-foreground">Expected</p><MoneyAmount amount={item.expectedAmount} currency={item.currency} className="text-lg font-bold" /></div>
          <div className="text-center"><p className="text-xs text-muted-foreground">Actual</p><MoneyAmount amount={item.actualAmount} currency={item.currency} className="text-lg font-bold" /></div>
          <div className="text-center"><p className="text-xs text-muted-foreground">Difference</p><MoneyAmount amount={item.differenceAmount} currency={item.currency} className={`text-lg font-bold ${item.differenceAmount !== 0 ? 'text-red-600' : 'text-green-600'}`} /></div>
        </CardContent>
      </Card>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Reference Type</span><span>{item.referenceType}</span></div>
            {item.walletId && <div className="flex justify-between"><span className="text-muted-foreground">Wallet</span><Link href={`/admin/wallets/${item.walletId}`} className="text-primary hover:underline font-mono text-xs">{item.walletId.slice(0, 12)}…</Link></div>}
            {item.paymentIntentId && <div className="flex justify-between"><span className="text-muted-foreground">Intent</span><Link href={`/admin/payment-intents/${item.paymentIntentId}`} className="text-primary hover:underline font-mono text-xs">{item.paymentIntentId.slice(0, 12)}…</Link></div>}
            <div className="flex justify-between"><span className="text-muted-foreground">Detected</span><span>{new Date(item.detectedAt).toLocaleString()}</span></div>
            {item.resolvedAt && <div className="flex justify-between"><span className="text-muted-foreground">Resolved</span><span>{new Date(item.resolvedAt).toLocaleString()}</span></div>}
          </CardContent>
        </Card>
      </div>

      {item.detectionReason && <Card><CardContent className="p-4"><p className="text-sm font-medium">Detection Reason</p><p className="mt-1 text-xs text-muted-foreground">{item.detectionReason}</p></CardContent></Card>}
      {item.resolutionNote && <Card><CardContent className="p-4"><p className="text-sm font-medium">Resolution</p><p className="mt-1 text-xs text-muted-foreground">{item.resolutionNote}</p></CardContent></Card>}
      {item.expectedValues && <SafePayloadViewer data={item.expectedValues} title="Expected Values" />}
      {item.actualValues && <SafePayloadViewer data={item.actualValues} title="Actual Values" />}
    </div>
  );
}
