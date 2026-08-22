'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { PermissionGuard } from '@/components/shared/permission-guards';
import { MaskedCardNumber, MaskedIban } from '@/components/shared/masked-value';
import { WithdrawalActionDialog } from '@/components/shared/withdrawal-action-dialog';
import { getWithdrawal } from '@/api/withdrawals';
import { CheckCircle, XCircle, HelpCircle, Play, Banknote, AlertTriangle, Lock, Unlock, ArrowRight } from 'lucide-react';

export default function WithdrawalDetailPage() {
  const { withdrawalRequestId } = useParams<{ withdrawalRequestId: string }>();
  const [action, setAction] = useState<'approve' | 'reject' | 'request-more-info' | 'mark-processing' | 'mark-paid' | 'mark-failed' | null>(null);

  const { data: withdrawal, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['withdrawal', withdrawalRequestId],
    queryFn: () => getWithdrawal(withdrawalRequestId),
  });

  if (isLoading) return <DetailPageSkeleton cards={4} />;
  if (isError || !withdrawal) return <ErrorState error={(error as Error)?.message || t('notFound.withdrawal')} onRetry={() => refetch()} />;

  const canAct = (status: string) => withdrawal.status === status;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <Link href="/admin/withdrawals" className="text-sm text-muted-foreground hover:text-foreground">← Withdrawals</Link>
          <div className="mt-1 flex items-center gap-3"><h1 className="text-xl font-bold">Withdrawal Request</h1><StatusBadge status={withdrawal.status} /></div>
          <p className="text-xs font-mono text-muted-foreground">{withdrawal.id}</p>
        </div>
        <PermissionGuard permission="payment.withdrawal.view_admin">
          <div className="flex gap-2 flex-wrap">
            {canAct('pending_review') && (
              <>
                <Button variant="outline" size="sm" onClick={() => setAction('approve')} className="text-green-600"><CheckCircle className="mr-1 h-3.5 w-3.5" /> Approve</Button>
                <Button variant="outline" size="sm" onClick={() => setAction('reject')} className="text-red-600"><XCircle className="mr-1 h-3.5 w-3.5" /> Reject</Button>
                <Button variant="outline" size="sm" onClick={() => setAction('request-more-info')}><HelpCircle className="mr-1 h-3.5 w-3.5" /> Request Info</Button>
              </>
            )}
            {canAct('approved') && (
              <Button variant="outline" size="sm" onClick={() => setAction('mark-processing')}><Play className="mr-1 h-3.5 w-3.5" /> Mark Processing</Button>
            )}
            {canAct('processing_payout') && (
              <>
                <Button variant="outline" size="sm" onClick={() => setAction('mark-paid')} className="text-green-600"><Banknote className="mr-1 h-3.5 w-3.5" /> Mark Paid</Button>
                <Button variant="outline" size="sm" onClick={() => setAction('mark-failed')} className="text-red-600"><AlertTriangle className="mr-1 h-3.5 w-3.5" /> Mark Failed</Button>
              </>
            )}
          </div>
        </PermissionGuard>
      </div>

      {/* Amount Summary */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card><CardHeader className="pb-1"><CardTitle className="text-xs font-medium text-muted-foreground">Amount</CardTitle></CardHeader><CardContent><MoneyAmount amount={withdrawal.amount} currency={withdrawal.currency} className="text-lg font-bold" /></CardContent></Card>
        <Card><CardHeader className="pb-1"><CardTitle className="text-xs font-medium text-muted-foreground">Fee</CardTitle></CardHeader><CardContent><MoneyAmount amount={withdrawal.feeAmount} currency={withdrawal.currency} className="text-lg font-bold" /></CardContent></Card>
        <Card><CardHeader className="pb-1"><CardTitle className="text-xs font-medium text-muted-foreground">Net</CardTitle></CardHeader><CardContent><MoneyAmount amount={withdrawal.netAmount} currency={withdrawal.currency} className="text-lg font-bold" /></CardContent></Card>
        <Card><CardHeader className="pb-1"><CardTitle className="text-xs font-medium text-muted-foreground">Status</CardTitle></CardHeader><CardContent><StatusBadge status={withdrawal.status} /></CardContent></Card>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <Card>
          <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Owner</span><span className="text-xs">{withdrawal.ownerType}: {withdrawal.ownerId}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Tenant</span><span className="font-mono text-xs">{withdrawal.tenantId}</span></div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Wallet</span>
              <Link href={`/admin/wallets/${withdrawal.walletId}`} className="font-mono text-xs text-primary hover:underline">{withdrawal.walletId.slice(0, 12)}…</Link>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Payout</span>
              <Link href={`/admin/payout-accounts/${withdrawal.payoutAccountId}`} className="font-mono text-xs text-primary hover:underline">{withdrawal.payoutAccountId.slice(0, 12)}…</Link>
            </div>
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span className="text-xs">{new Date(withdrawal.createdAt).toLocaleString()}</span></div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Payout Account</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Holder</span><span className="text-xs">{withdrawal.holderName}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Bank</span><span className="text-xs">{withdrawal.bankName || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Type</span><span className="text-xs capitalize">{withdrawal.accountType?.replace(/_/g, ' ')}</span></div>
            {withdrawal.cardMasked && <div className="flex justify-between"><span className="text-muted-foreground">Card</span><MaskedCardNumber value={withdrawal.cardMasked} /></div>}
            {withdrawal.ibanMasked && <div className="flex justify-between"><span className="text-muted-foreground">IBAN</span><MaskedIban value={withdrawal.ibanMasked} /></div>}
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Timeline</CardTitle></CardHeader>
          <CardContent>
            {withdrawal.timeline?.length ? (
              <div className="relative space-y-0">
                {withdrawal.timeline.map((event, i) => (
                  <div key={i} className="flex gap-3 pb-3">
                    <div className="relative flex flex-col items-center">
                      <div className={`h-2.5 w-2.5 rounded-full border-2 ${i === withdrawal.timeline.length - 1 ? 'border-primary bg-primary' : 'border-muted-foreground bg-muted-foreground'}`} />
                      {i < withdrawal.timeline.length - 1 && <div className="w-0.5 flex-1 bg-border" />}
                    </div>
                    <div className="flex-1"><p className="text-xs font-medium">{event.label}</p><p className="text-xs text-muted-foreground">{new Date(event.timestamp).toLocaleString()}</p></div>
                  </div>
                ))}
              </div>
            ) : <p className="text-sm text-muted-foreground">No timeline data</p>}
          </CardContent>
        </Card>
      </div>

      {/* Wallet Lock/Release/Capture */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card className={withdrawal.lockTransactionId ? 'border-blue-500' : ''}>
          <CardContent className="flex items-center gap-3 p-4">
            <Lock className="h-5 w-5 text-blue-600" />
            <div><p className="text-xs font-medium">Lock Transaction</p>
              {withdrawal.lockTransactionId ? <p className="font-mono text-xs">{withdrawal.lockTransactionId.slice(0, 12)}…</p> : <p className="text-xs text-muted-foreground">—</p>}
            </div>
          </CardContent>
        </Card>
        <Card className={withdrawal.releaseTransactionId ? 'border-amber-500' : ''}>
          <CardContent className="flex items-center gap-3 p-4">
            <Unlock className="h-5 w-5 text-amber-600" />
            <div><p className="text-xs font-medium">Release Transaction</p>
              {withdrawal.releaseTransactionId ? <p className="font-mono text-xs">{withdrawal.releaseTransactionId.slice(0, 12)}…</p> : <p className="text-xs text-muted-foreground">—</p>}
            </div>
          </CardContent>
        </Card>
        <Card className={withdrawal.captureTransactionId ? 'border-green-500' : ''}>
          <CardContent className="flex items-center gap-3 p-4">
            <ArrowRight className="h-5 w-5 text-green-600" />
            <div><p className="text-xs font-medium">Capture Transaction</p>
              {withdrawal.captureTransactionId ? <p className="font-mono text-xs">{withdrawal.captureTransactionId.slice(0, 12)}…</p> : <p className="text-xs text-muted-foreground">—</p>}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Payout Record */}
      {withdrawal.payoutRecord && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Payout Record</CardTitle></CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2 text-sm">
                <div className="flex justify-between"><span className="text-muted-foreground">ID</span><span className="font-mono text-xs">{withdrawal.payoutRecord.id}</span></div>
                <div className="flex justify-between"><span className="text-muted-foreground">Provider</span><span className="text-xs">{withdrawal.payoutRecord.provider}</span></div>
                <div className="flex justify-between"><span className="text-muted-foreground">Status</span><StatusBadge status={withdrawal.payoutRecord.status} /></div>
                <div className="flex justify-between"><span className="text-muted-foreground">Amount</span><MoneyAmount amount={withdrawal.payoutRecord.amount} currency={withdrawal.payoutRecord.currency} /></div>
                <div className="flex justify-between"><span className="text-muted-foreground">Tracking #</span><span className="font-mono text-xs">{withdrawal.payoutRecord.bankTrackingNumber || '—'}</span></div>
                <div className="flex justify-between"><span className="text-muted-foreground">Reference</span><span className="font-mono text-xs">{withdrawal.payoutRecord.bankReferenceId || '—'}</span></div>
                {withdrawal.payoutRecord.paidAt && <div className="flex justify-between"><span className="text-muted-foreground">Paid</span><span className="text-xs">{new Date(withdrawal.payoutRecord.paidAt).toLocaleString()}</span></div>}
              </div>
              {withdrawal.payoutRecord.responsePayload && <SafePayloadViewer data={withdrawal.payoutRecord.responsePayload} title="Bank Response" />}
            </div>
          </CardContent>
        </Card>
      )}

      {withdrawal.adminNote && (
        <Card><CardContent className="p-4"><span className="text-xs font-medium text-muted-foreground">Admin Note:</span><p className="mt-1 text-sm">{withdrawal.adminNote}</p></CardContent></Card>
      )}

      {action && withdrawal && (
        <WithdrawalActionDialog withdrawal={withdrawal} action={action} open onClose={() => setAction(null)} />
      )}
    </div>
  );
}
