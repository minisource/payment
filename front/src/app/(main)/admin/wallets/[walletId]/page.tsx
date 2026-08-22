'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { DataTable, DataTableColumn } from '@/components/shared/data-table';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { ConfirmDialog } from '@/components/shared/components';
import { Loading, ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { PermissionGuard } from '@/components/shared/permission-guards';
import { toast } from 'sonner';
import {
  getWallet, adminCreditWallet, adminDebitWallet,
  verifyWalletHashChain, runWalletDeepCheck,
  type WalletTransaction, type WalletAdjustmentRequest,
  type WalletAdjustmentResponse, type HashChainVerificationResult,
  type WalletDeepCheckResult,
} from '@/api/wallets';
import type { LedgerEntry, PaymentIntent } from '@/types/payment';
import {
  Plus, Minus, AlertTriangle, CheckCircle, Hash, Activity,
} from 'lucide-react';

export default function WalletDetailPage() {
  const { walletId } = useParams<{ walletId: string }>();
  const queryClient = useQueryClient();
  const [adjustOpen, setAdjustOpen] = useState<'credit' | 'debit' | null>(null);
  const [adjustAmount, setAdjustAmount] = useState('');
  const [adjustReason, setAdjustReason] = useState('');
  const [hashResult, setHashResult] = useState<HashChainVerificationResult | null>(null);
  const [deepCheckResult, setDeepCheckResult] = useState<WalletDeepCheckResult | null>(null);

  const { data: wallet, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['wallet', walletId],
    queryFn: () => getWallet(walletId),
  });

  const adjustMutation = useMutation({
    mutationFn: (req: { type: 'credit' | 'debit'; data: WalletAdjustmentRequest }) =>
      req.type === 'credit' ? adminCreditWallet(walletId, req.data) : adminDebitWallet(walletId, req.data),
    onSuccess: (res: WalletAdjustmentResponse) => {
      if (res.approvalRequestId) {
        toast.success('Approval request created', { description: `Request ID: ${res.approvalRequestId}` });
      } else {
        toast.success('Adjustment applied');
      }
      setAdjustOpen(null); setAdjustAmount(''); setAdjustReason('');
      queryClient.invalidateQueries({ queryKey: ['wallet', walletId] });
    },
    onError: (err: Error) => toast.error(err.message || 'Adjustment failed'),
  });

  const isValidAmount = adjustAmount && !isNaN(Number(adjustAmount)) && Number(adjustAmount) > 0;

  const handleAdjust = () => {
    if (!isValidAmount || !adjustReason || !wallet) return;
    adjustMutation.mutate({
      type: adjustOpen as 'credit' | 'debit',
      data: { amount: adjustAmount, currency: wallet.currency, reason: adjustReason },
    });
  };

  const handleVerifyHash = async () => {
    try { const res = await verifyWalletHashChain(walletId); setHashResult(res); } catch (e) { toast.error((e as Error).message); }
  };
  const handleDeepCheck = async () => {
    try { const res = await runWalletDeepCheck(walletId); setDeepCheckResult(res); } catch (e) { toast.error((e as Error).message); }
  };

  if (isLoading) return <DetailPageSkeleton cards={5} />;
  if (isError || !wallet) return <ErrorState error={(error as Error)?.message || t('notFound.wallet')} onRetry={() => refetch()} />;

  const txColumns: DataTableColumn<WalletTransaction>[] = [
    { key: 'type', header: 'Type', render: (t) => <span className="text-xs capitalize">{t.type.replace(/_/g, ' ')}</span> },
    { key: 'amount', header: 'Amount', render: (t) => <MoneyAmount amount={t.amount} currency={t.currency} className="text-xs" /> },
    { key: 'status', header: 'Status', render: (t) => <StatusBadge status={t.status} /> },
    { key: 'reference', header: 'Reference', render: (t) => t.referenceType ? <span className="text-xs">{t.referenceType}: {t.referenceId?.slice(0, 12)}</span> : <span className="text-xs text-muted-foreground">—</span> },
  ];

  const ledgerColumns: DataTableColumn<LedgerEntry>[] = [
    { key: 'entryType', header: 'Type', render: (e) => <span className="text-xs capitalize">{e.entryType.replace(/_/g, ' ')}</span> },
    { key: 'direction', header: 'Dir', render: (e) => <span className={e.direction === 'credit' ? 'text-green-600 text-xs' : 'text-red-600 text-xs'}>{e.direction}</span> },
    { key: 'amount', header: 'Amount', render: (e) => <MoneyAmount amount={e.amount} currency={e.currency} className="text-xs" /> },
    { key: 'entryHash', header: 'Hash', render: (e) => <span className="font-mono text-xs text-muted-foreground">{e.entryHash?.slice(0, 10)}…</span> },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Link href="/admin/wallets" className="text-sm text-muted-foreground hover:text-foreground">← Wallets</Link>
        <h1 className="text-xl font-bold">Wallet {wallet.id.slice(0, 12)}…</h1>
        <StatusBadge status={wallet.status} />
      </div>

      {/* Balance Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-1"><CardTitle className="text-xs font-medium text-muted-foreground">Available</CardTitle></CardHeader>
          <CardContent><MoneyAmount amount={wallet.availableBalance} currency={wallet.currency} className="text-lg font-bold" /></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-1"><CardTitle className="text-xs font-medium text-muted-foreground">Locked</CardTitle></CardHeader>
          <CardContent><MoneyAmount amount={wallet.lockedBalance} currency={wallet.currency} className="text-lg font-bold" /></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-1"><CardTitle className="text-xs font-medium text-muted-foreground">Pending</CardTitle></CardHeader>
          <CardContent><MoneyAmount amount={wallet.pendingBalance} currency={wallet.currency} className="text-lg font-bold" /></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-1"><CardTitle className="text-xs font-medium text-muted-foreground">Total</CardTitle></CardHeader>
          <CardContent><MoneyAmount amount={wallet.availableBalance + wallet.lockedBalance + wallet.pendingBalance} currency={wallet.currency} className="text-lg font-bold" /></CardContent>
        </Card>
      </div>

      {/* Wallet Info + Actions */}
      <div className="grid gap-6 md:grid-cols-3">
        <Card className="md:col-span-2">
          <CardHeader><CardTitle className="text-sm">Wallet Information</CardTitle></CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div><span className="text-muted-foreground">ID:</span> <span className="font-mono text-xs">{wallet.id}</span></div>
              <div><span className="text-muted-foreground">Tenant:</span> <span className="font-mono text-xs">{wallet.tenantId}</span></div>
              <div><span className="text-muted-foreground">Owner:</span> <span className="text-xs capitalize">{wallet.ownerType}: {wallet.ownerId}</span></div>
              <div><span className="text-muted-foreground">Currency:</span> <span className="font-mono text-xs">{wallet.currency}</span></div>
              <div><span className="text-muted-foreground">Version:</span> <span className="font-mono text-xs">{wallet.version}</span></div>
              <div><span className="text-muted-foreground">Created:</span> <span className="text-xs">{new Date(wallet.createdAt).toLocaleString()}</span></div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Admin Actions</CardTitle></CardHeader>
          <CardContent className="space-y-2">
            <PermissionGuard permission="payment.wallet.adjust_admin">
              <Button variant="outline" size="sm" className="w-full justify-start" onClick={() => setAdjustOpen('credit')}>
                <Plus className="mr-2 h-3.5 w-3.5 text-green-600" /> Credit Adjustment
              </Button>
              <Button variant="outline" size="sm" className="w-full justify-start" onClick={() => setAdjustOpen('debit')}>
                <Minus className="mr-2 h-3.5 w-3.5 text-red-600" /> Debit Adjustment
              </Button>
            </PermissionGuard>
            <Button variant="outline" size="sm" className="w-full justify-start" onClick={handleVerifyHash}>
              <Hash className="mr-2 h-3.5 w-3.5" /> Verify Hash Chain
            </Button>
            <Button variant="outline" size="sm" className="w-full justify-start" onClick={handleDeepCheck}>
              <Activity className="mr-2 h-3.5 w-3.5" /> Run Deep Check
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Hash / Deep Check results */}
      {hashResult && (
        <Card className={hashResult.valid ? 'border-green-500' : 'border-red-500'}>
          <CardContent className="flex items-center gap-3 p-4">
            {hashResult.valid ? <CheckCircle className="h-5 w-5 text-green-600" /> : <AlertTriangle className="h-5 w-5 text-red-600" />}
            <div>
              <p className="text-sm font-medium">{hashResult.valid ? 'Hash chain is valid' : 'Hash chain is invalid'}</p>
              {!hashResult.valid && <p className="text-xs text-muted-foreground">{hashResult.invalidEntries} invalid entries found</p>}
            </div>
          </CardContent>
        </Card>
      )}
      {deepCheckResult && (
        <Card className={deepCheckResult.consistent ? 'border-green-500' : 'border-red-500'}>
          <CardContent className="p-4">
            <p className="text-sm font-medium">{deepCheckResult.consistent ? 'Wallet is consistent' : 'Wallet has consistency issues'}</p>
            {deepCheckResult.issues.map((issue, i) => (
              <p key={i} className="mt-1 text-xs text-muted-foreground">• {issue.description}</p>
            ))}
          </CardContent>
        </Card>
      )}

      {/* Recent Transactions */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-sm">Recent Transactions</CardTitle>
          <Link href={`/admin/wallets/${walletId}/transactions`} className="text-xs text-primary hover:underline">View all →</Link>
        </CardHeader>
        <CardContent className="p-0">
          <DataTable columns={txColumns} data={wallet.transactions?.slice(0, 10) || []} keyExtractor={(t) => t.id} compact
            isEmpty={!wallet.transactions?.length} emptyMessage="No transactions" />
        </CardContent>
      </Card>

      {/* Recent Ledger */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-sm">Recent Ledger Entries</CardTitle>
          <Link href={`/admin/wallets/${walletId}/ledger`} className="text-xs text-primary hover:underline">View all →</Link>
        </CardHeader>
        <CardContent className="p-0">
          <DataTable columns={ledgerColumns} data={wallet.recentLedgerEntries?.slice(0, 10) || []} keyExtractor={(e) => e.id} compact
            isEmpty={!wallet.recentLedgerEntries?.length} emptyMessage="No ledger entries" />
        </CardContent>
      </Card>

      {/* Related Payment Intents */}
      {wallet.relatedPaymentIntents && wallet.relatedPaymentIntents.length > 0 && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Related Payment Intents</CardTitle></CardHeader>
          <CardContent className="p-0">
            <DataTable
              columns={[
                { key: 'id', header: 'ID', render: (p: PaymentIntent) => <Link href={`/admin/payment-intents/${p.id}`} className="font-mono text-xs text-primary hover:underline">{p.id.slice(0, 12)}…</Link> },
                { key: 'amount', header: 'Amount', render: (p: PaymentIntent) => <MoneyAmount amount={p.amount} currency={p.currency} className="text-xs" /> },
                { key: 'status', header: 'Status', render: (p: PaymentIntent) => <StatusBadge status={p.status} /> },
                { key: 'date', header: 'Date', render: (p: PaymentIntent) => <span className="text-xs">{new Date(p.createdAt).toLocaleDateString()}</span> },
              ]}
              data={wallet.relatedPaymentIntents}
              keyExtractor={(p) => p.id}
              compact
            />
          </CardContent>
        </Card>
      )}

      {/* Adjustment Dialog */}
      {adjustOpen && wallet && (
        <ConfirmDialog
          open
          onClose={() => { setAdjustOpen(null); setAdjustAmount(''); setAdjustReason(''); }}
          onConfirm={handleAdjust}
          title={`${adjustOpen === 'credit' ? 'Credit' : 'Debit'} Adjustment`}
          description={
            <div className="mt-3 space-y-3">
              <p className="text-sm text-muted-foreground">
                {adjustOpen === 'debit' && '⚠️ This will reduce the wallet balance. '}
                Currency: <strong>{wallet.currency}</strong>
              </p>
              <div>
                <label className="text-xs font-medium">Amount *</label>
                <input type="text" value={adjustAmount} onChange={(e) => setAdjustAmount(e.target.value)}
                  placeholder="e.g. 100000" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
              </div>
              <div>
                <label className="text-xs font-medium">Reason *</label>
                <input type="text" value={adjustReason} onChange={(e) => setAdjustReason(e.target.value)}
                  placeholder="Reason for adjustment" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
              </div>
            </div>
          }
          confirmLabel={adjustMutation.isPending ? 'Processing…' : `Confirm ${adjustOpen === 'credit' ? 'Credit' : 'Debit'}`}
          destructive={adjustOpen === 'debit'}
          confirmDisabled={!isValidAmount || !adjustReason || adjustMutation.isPending}
        />
      )}
    </div>
  );
}
