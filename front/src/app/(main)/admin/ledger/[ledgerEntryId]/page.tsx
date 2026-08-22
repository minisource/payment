'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { MoneyAmount } from '@/components/shared/money-amount';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { getLedgerEntry } from '@/api/ledger';
import { Shield, ArrowLeftRight, Hash } from 'lucide-react';

export default function LedgerEntryDetailPage() {
  const { ledgerEntryId } = useParams<{ ledgerEntryId: string }>();

  const { data: entry, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['ledger-entry', ledgerEntryId],
    queryFn: () => getLedgerEntry(ledgerEntryId),
  });

  if (isLoading) return <DetailPageSkeleton cards={2} />;
  if (isError || !entry) return <ErrorState error={(error as Error)?.message || 'Ledger entry not found'} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <div>
        <Link href="/admin/ledger" className="text-sm text-muted-foreground hover:text-foreground">← Ledger Explorer</Link>
        <h1 className="mt-1 text-xl font-bold">Ledger Entry</h1>
        <p className="text-xs font-mono text-muted-foreground">{entry.id}</p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Entry Details</CardTitle></CardHeader>
          <CardContent className="space-y-3 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Entry Type</span><span className="font-medium capitalize">{entry.entryType.replace(/_/g, ' ')}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Direction</span><span className={entry.direction === 'credit' ? 'font-medium text-green-600' : 'font-medium text-red-600'}>{entry.direction.toUpperCase()}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Amount</span><MoneyAmount amount={entry.amount} currency={entry.currency} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Wallet</span><Link href={`/admin/wallets/${entry.walletAccountId}`} className="font-mono text-xs text-primary hover:underline">{entry.walletAccountId}</Link></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Tenant</span><span className="font-mono text-xs">{entry.tenantId}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Reference</span><span className="text-xs">{entry.referenceType ? `${entry.referenceType}: ${entry.referenceId}` : '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Reason</span><span className="text-xs">{entry.reason || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Date</span><span className="text-xs">{new Date(entry.createdAt).toLocaleString()}</span></div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Balances</CardTitle></CardHeader>
          <CardContent className="space-y-3 text-sm">
            <div className="rounded-md border p-3 space-y-2">
              <p className="text-xs font-medium text-muted-foreground">Available Balance</p>
              <div className="flex items-center gap-2">
                <MoneyAmount amount={entry.balanceAvailableBefore} currency={entry.currency} className="text-xs" />
                <ArrowLeftRight className="h-3 w-3 text-muted-foreground" />
                <MoneyAmount amount={entry.balanceAvailableAfter} currency={entry.currency} className="text-xs font-bold" />
              </div>
            </div>
            <div className="rounded-md border p-3 space-y-2">
              <p className="text-xs font-medium text-muted-foreground">Locked Balance</p>
              <div className="flex items-center gap-2">
                <MoneyAmount amount={entry.balanceLockedBefore} currency={entry.currency} className="text-xs" />
                <ArrowLeftRight className="h-3 w-3 text-muted-foreground" />
                <MoneyAmount amount={entry.balanceLockedAfter} currency={entry.currency} className="text-xs font-bold" />
              </div>
            </div>
            <div className="rounded-md border p-3 space-y-2">
              <p className="text-xs font-medium text-muted-foreground">Pending Balance</p>
              <div className="flex items-center gap-2">
                <MoneyAmount amount={entry.balancePendingBefore} currency={entry.currency} className="text-xs" />
                <ArrowLeftRight className="h-3 w-3 text-muted-foreground" />
                <MoneyAmount amount={entry.balancePendingAfter} currency={entry.currency} className="text-xs font-bold" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><CardTitle className="text-sm flex items-center gap-2"><Hash className="h-4 w-4" /> Hash Information</CardTitle></CardHeader>
        <CardContent className="space-y-2 text-sm">
          <div><span className="text-muted-foreground">Previous Entry Hash:</span> <span className="font-mono text-xs break-all">{entry.previousEntryHash || '—'}</span></div>
          <div><span className="text-muted-foreground">Entry Hash:</span> <span className="font-mono text-xs break-all">{entry.entryHash || '—'}</span></div>
          {entry.hashAlgorithm && <div><span className="text-muted-foreground">Algorithm:</span> <span className="font-mono text-xs">{entry.hashAlgorithm}</span></div>}
          {entry.hashVersion != null && <div><span className="text-muted-foreground">Version:</span> <span className="font-mono text-xs">{entry.hashVersion}</span></div>}
        </CardContent>
      </Card>

      {entry.metadata && <SafePayloadViewer data={entry.metadata} title="Metadata" />}

      <Card className="border-dashed bg-muted/20">
        <CardContent className="flex items-center gap-3 p-3">
          <Shield className="h-4 w-4 text-muted-foreground" />
          <p className="text-xs text-muted-foreground">This ledger entry is append-only and cannot be modified or deleted.</p>
        </CardContent>
      </Card>
    </div>
  );
}
