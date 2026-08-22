'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { ConfirmDialog } from '@/components/shared/components';
import { PermissionGuard } from '@/components/shared/permission-guards';
import { toast } from 'sonner';
import { getPaymentTransaction, manualVerifyPaymentTransaction } from '@/api/payment-transactions';
import { ArrowLeftRight, Shield, RefreshCw } from 'lucide-react';

export default function PaymentTransactionDetailPage() {
  const { paymentTransactionId } = useParams<{ paymentTransactionId: string }>();
  const queryClient = useQueryClient();
  const [verifyOpen, setVerifyOpen] = useState(false);

  const { data: tx, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['payment-transaction', paymentTransactionId],
    queryFn: () => getPaymentTransaction(paymentTransactionId),
  });

  const verifyMutation = useMutation({
    mutationFn: () => manualVerifyPaymentTransaction(paymentTransactionId),
    onSuccess: (res) => {
      toast.success(res.message || 'Verification complete', { description: `New status: ${res.status}` });
      setVerifyOpen(false);
      queryClient.invalidateQueries({ queryKey: ['payment-transaction', paymentTransactionId] });
    },
    onError: (err: Error) => toast.error(err.message || 'Verification failed'),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !tx) return <ErrorState error={(error as Error)?.message || 'Transaction not found'} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <Link href="/admin/payment-transactions" className="text-sm text-muted-foreground hover:text-foreground">← Gateway Transactions</Link>
          <h1 className="mt-1 text-xl font-bold">Payment Transaction</h1>
          <p className="text-xs font-mono text-muted-foreground">{tx.id}</p>
        </div>
        <PermissionGuard permission="payment.gateway.transaction.verify_admin">
          <Button variant="outline" size="sm" onClick={() => setVerifyOpen(true)} disabled={tx.status === 'verified' || tx.status === 'succeeded'}>
            <RefreshCw className="mr-2 h-3.5 w-3.5" /> Manual Verify
          </Button>
        </PermissionGuard>
      </div>

      {/* Summary */}
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm flex items-center gap-2"><ArrowLeftRight className="h-4 w-4" /> Transaction Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">ID</span><span className="font-mono text-xs">{tx.id}</span></div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Payment Intent</span>
              <Link href={`/admin/payment-intents/${tx.paymentIntentId}`} className="font-mono text-xs text-primary hover:underline">{tx.paymentIntentId.slice(0, 12)}…</Link>
            </div>
            <div className="flex justify-between"><span className="text-muted-foreground">Provider</span><span className="text-xs font-medium">{tx.providerCode}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Gateway Config</span><span className="font-mono text-xs">{tx.gatewayConfigId}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Amount</span><MoneyAmount amount={tx.amount} currency={tx.currency} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Status</span><StatusBadge status={tx.status} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Tenant</span><span className="font-mono text-xs">{tx.tenantId}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Authority</span><span className="font-mono text-xs">{tx.authority || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Reference</span><span className="font-mono text-xs">{tx.gatewayReferenceId || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Trace Number</span><span className="font-mono text-xs">{tx.traceNumber || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Card (masked)</span><span className="font-mono text-xs">{tx.cardPanMasked || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Response Code</span><span className="font-mono text-xs">{tx.responseCode || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span className="text-xs">{new Date(tx.createdAt).toLocaleString()}</span></div>
            {tx.verifiedAt && <div className="flex justify-between"><span className="text-muted-foreground">Verified</span><span className="text-xs text-green-600">{new Date(tx.verifiedAt).toLocaleString()}</span></div>}
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm flex items-center gap-2"><Shield className="h-4 w-4" /> Timeline</CardTitle></CardHeader>
          <CardContent>
            {tx.timeline?.length ? (
              <div className="relative space-y-0">
                {tx.timeline.map((event, i) => (
                  <div key={i} className="flex gap-3 pb-3">
                    <div className="relative flex flex-col items-center">
                      <div className={`h-2.5 w-2.5 rounded-full border-2 ${i === tx.timeline.length - 1 ? 'border-primary bg-primary' : 'border-muted-foreground bg-muted-foreground'}`} />
                      {i < tx.timeline.length - 1 && <div className="w-0.5 flex-1 bg-border" />}
                    </div>
                    <div>
                      <p className="text-xs font-medium">{event.label}</p>
                      <p className="text-xs text-muted-foreground">{new Date(event.timestamp).toLocaleString()}</p>
                    </div>
                  </div>
                ))}
              </div>
            ) : <p className="text-sm text-muted-foreground">No timeline data</p>}
          </CardContent>
        </Card>
      </div>

      {/* Payloads */}
      <div className="grid gap-6 md:grid-cols-2">
        {tx.callbackPayload && <SafePayloadViewer data={tx.callbackPayload} title="Callback Payload" />}
        {tx.verifyPayload && <SafePayloadViewer data={tx.verifyPayload} title="Verify Payload" />}
      </div>

      {/* Related Payment Intent */}
      {tx.paymentIntent && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Related Payment Intent</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">ID</span><Link href={`/admin/payment-intents/${tx.paymentIntent.id}`} className="font-mono text-xs text-primary hover:underline">{tx.paymentIntent.id}</Link></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Amount</span><MoneyAmount amount={tx.paymentIntent.amount} currency={tx.paymentIntent.currency} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Status</span><StatusBadge status={tx.paymentIntent.status} /></div>
          </CardContent>
        </Card>
      )}

      {/* Manual Verify Dialog */}
      <ConfirmDialog
        open={verifyOpen}
        onClose={() => setVerifyOpen(false)}
        onConfirm={() => verifyMutation.mutate()}
        title="Manual Verify"
        description="Manual verify may contact the gateway provider and update the payment status if successful. This action cannot be undone."
        confirmLabel={verifyMutation.isPending ? 'Verifying…' : 'Confirm Verify'}
        destructive
        confirmDisabled={verifyMutation.isPending}
      />
    </div>
  );
}
