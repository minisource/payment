'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { DataTable, DataTableColumn } from '@/components/shared/data-table';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { getPaymentIntent, type TimelineEvent } from '@/api/payment-intents';
import type { PaymentTransaction } from '@/types/payment';
import { ArrowLeftRight, Clock, Wallet } from 'lucide-react';

export default function PaymentIntentDetailPage() {
  const { paymentIntentId } = useParams<{ paymentIntentId: string }>();

  const { data: intent, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['payment-intent', paymentIntentId],
    queryFn: () => getPaymentIntent(paymentIntentId),
  });

  if (isLoading) return <DetailPageSkeleton cards={4} />;
  if (isError || !intent) return <ErrorState error={(error as Error)?.message || t('notFound.paymentIntent')} onRetry={() => refetch()} />;

  const txColumns: DataTableColumn<PaymentTransaction>[] = [
    { key: 'id', header: 'Transaction ID', render: (t) => <Link href={`/admin/payment-transactions/${t.id}`} className="font-mono text-xs text-primary hover:underline">{t.id.slice(0, 12)}…</Link> },
    { key: 'provider', header: 'Provider', render: (t) => <span className="text-xs">{t.providerCode}</span> },
    { key: 'amount', header: 'Amount', render: (t) => <MoneyAmount amount={t.amount} currency={t.currency} className="text-xs" /> },
    { key: 'status', header: 'Status', render: (t) => <StatusBadge status={t.status} /> },
    { key: 'verifiedAt', header: 'Verified', render: (t) => <span className="text-xs">{t.verifiedAt ? new Date(t.verifiedAt).toLocaleString() : '—'}</span> },
  ];

  return (
    <div className="space-y-6">
      <div>
        <Link href="/admin/payment-intents" className="text-sm text-muted-foreground hover:text-foreground">← Payment Intents</Link>
        <h1 className="mt-1 text-xl font-bold">Payment Intent</h1>
        <p className="text-xs font-mono text-muted-foreground">{intent.id}</p>
      </div>

      {/* Summary + Status Timeline */}
      <div className="grid gap-6 md:grid-cols-3">
        <Card className="md:col-span-1">
          <CardHeader><CardTitle className="text-sm flex items-center gap-2"><ArrowLeftRight className="h-4 w-4" /> Summary</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Amount</span><MoneyAmount amount={intent.amount} currency={intent.currency} /></div>
            {intent.feeAmount != null && <div className="flex justify-between"><span className="text-muted-foreground">Fee</span><MoneyAmount amount={intent.feeAmount} currency={intent.currency} /></div>}
            {intent.netAmount != null && <div className="flex justify-between"><span className="text-muted-foreground">Net</span><MoneyAmount amount={intent.netAmount} currency={intent.currency} /></div>}
            <div className="flex justify-between"><span className="text-muted-foreground">Status</span><StatusBadge status={intent.status} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Provider</span><span className="text-xs">{intent.providerCode || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Wallet Behavior</span><span className="text-xs capitalize">{intent.walletBehavior.replace(/_/g, ' ')}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Tenant</span><span className="font-mono text-xs">{intent.tenantId}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span className="text-xs">{new Date(intent.createdAt).toLocaleString()}</span></div>
            {intent.succeededAt && <div className="flex justify-between"><span className="text-muted-foreground">Succeeded</span><span className="text-xs text-green-600">{new Date(intent.succeededAt).toLocaleString()}</span></div>}
            {intent.failedAt && <div className="flex justify-between"><span className="text-muted-foreground">Failed</span><span className="text-xs text-red-600">{new Date(intent.failedAt).toLocaleString()}</span></div>}
          </CardContent>
        </Card>

        <Card className="md:col-span-1">
          <CardHeader><CardTitle className="text-sm flex items-center gap-2"><Clock className="h-4 w-4" /> Timeline</CardTitle></CardHeader>
          <CardContent>
            {intent.timeline?.length ? (
              <div className="relative space-y-0">
                {intent.timeline.map((event: TimelineEvent, i: number) => (
                  <div key={i} className="flex gap-3 pb-3">
                    <div className="relative flex flex-col items-center">
                      <div className={`h-2.5 w-2.5 rounded-full border-2 ${i === intent.timeline.length - 1 ? 'border-primary bg-primary' : 'border-muted-foreground bg-muted-foreground'}`} />
                      {i < intent.timeline.length - 1 && <div className="w-0.5 flex-1 bg-border" />}
                    </div>
                    <div className="flex-1">
                      <p className="text-xs font-medium">{event.label}</p>
                      <p className="text-xs text-muted-foreground">{new Date(event.timestamp).toLocaleString()}</p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No timeline data available</p>
            )}
          </CardContent>
        </Card>

        <Card className="md:col-span-1">
          <CardHeader><CardTitle className="text-sm flex items-center gap-2"><Wallet className="h-4 w-4" /> Wallet Links</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            {intent.payerWalletId && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Payer Wallet</span>
                <Link href={`/admin/wallets/${intent.payerWalletId}`} className="font-mono text-xs text-primary hover:underline">{intent.payerWalletId.slice(0, 12)}…</Link>
              </div>
            )}
            {intent.recipientWalletId && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Recipient Wallet</span>
                <Link href={`/admin/wallets/${intent.recipientWalletId}`} className="font-mono text-xs text-primary hover:underline">{intent.recipientWalletId.slice(0, 12)}…</Link>
              </div>
            )}
            {intent.externalReferenceType && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">External Ref</span>
                <span className="text-xs">{intent.externalReferenceType}: {intent.externalReferenceId}</span>
              </div>
            )}
            {intent.paymentLinkId && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Payment Link</span>
                <span className="font-mono text-xs">{intent.paymentLinkId.slice(0, 12)}…</span>
              </div>
            )}
            {(!intent.payerWalletId && !intent.recipientWalletId && !intent.externalReferenceType) && (
              <p className="text-sm text-muted-foreground">No wallet links</p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Gateway Transactions */}
      <Card>
        <CardHeader><CardTitle className="text-sm">Gateway Transactions</CardTitle></CardHeader>
        <CardContent className="p-0">
          <DataTable columns={txColumns} data={intent.gatewayTransactions || []} keyExtractor={(t) => t.id}
            isEmpty={!intent.gatewayTransactions?.length} emptyMessage="No gateway transactions" />
        </CardContent>
      </Card>

      {/* Wallet Posting Result */}
      {intent.walletPostingResult && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Wallet Posting Result</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="grid grid-cols-3 gap-4">
              <div><span className="text-muted-foreground">Credit</span><p><MoneyAmount amount={intent.walletPostingResult.creditAmount} currency={intent.currency} /></p></div>
              <div><span className="text-muted-foreground">Debit</span><p><MoneyAmount amount={intent.walletPostingResult.debitAmount} currency={intent.currency} /></p></div>
              <div><span className="text-muted-foreground">Fee</span><p><MoneyAmount amount={intent.walletPostingResult.feeAmount} currency={intent.currency} /></p></div>
            </div>
          </CardContent>
        </Card>
      )}

      {intent.metadata && <SafePayloadViewer data={intent.metadata} title="Metadata" />}
    </div>
  );
}
