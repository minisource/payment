'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable, DataTableColumn, Pagination } from '@/components/shared/data-table';
import { MoneyAmount } from '@/components/shared/money-amount';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { listWalletLedger } from '@/api/wallets';
import type { LedgerEntry } from '@/types/payment';
import { Shield } from 'lucide-react';

const PAGE_SIZE = 25;

export default function WalletLedgerPage() {
  const { walletId } = useParams<{ walletId: string }>();
  const [skip, setSkip] = useState(0);

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['wallet-ledger', walletId, skip],
    queryFn: () => listWalletLedger(walletId, { skip, take: PAGE_SIZE }),
  });

  if (isLoading) return <DetailPageSkeleton cards={2} />;
  if (isError) return <ErrorState error={(error as Error)?.message} onRetry={() => refetch()} />;

  const columns: DataTableColumn<LedgerEntry>[] = [
    { key: 'id', header: 'Entry ID', render: (e) => <Link href={`/admin/ledger/${e.id}`} className="font-mono text-xs text-primary hover:underline">{e.id.slice(0, 12)}…</Link> },
    { key: 'entryType', header: 'Type', render: (e) => <span className="text-xs capitalize">{e.entryType.replace(/_/g, ' ')}</span> },
    { key: 'direction', header: 'Dir', render: (e) => <span className={e.direction === 'credit' ? 'text-green-600 text-xs font-medium' : 'text-red-600 text-xs font-medium'}>{e.direction.toUpperCase()}</span> },
    { key: 'amount', header: 'Amount', render: (e) => <MoneyAmount amount={e.amount} currency={e.currency} className="text-xs" /> },
    { key: 'balanceBefore', header: 'Before', render: (e) => <MoneyAmount amount={e.direction === 'credit' ? e.balanceAvailableBefore : e.balanceAvailableBefore} currency={e.currency} className="text-xs" /> },
    { key: 'balanceAfter', header: 'After', render: (e) => <MoneyAmount amount={e.direction === 'credit' ? e.balanceAvailableAfter : e.balanceAvailableAfter} currency={e.currency} className="text-xs" /> },
    { key: 'entryHash', header: 'Hash', render: (e) => <span className="font-mono text-xs text-muted-foreground">{e.entryHash?.slice(0, 10)}…</span> },
    { key: 'createdAt', header: 'Date', render: (e) => <span className="text-xs">{new Date(e.createdAt).toLocaleString()}</span> },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <Link href={`/admin/wallets/${walletId}`} className="text-sm text-muted-foreground hover:text-foreground">← Wallet {walletId.slice(0, 12)}…</Link>
          <h1 className="mt-1 text-2xl font-bold tracking-tight">Wallet Ledger</h1>
          <p className="text-sm text-muted-foreground">Immutable append-only ledger entries</p>
        </div>
      </div>

      <Card className="border-dashed bg-muted/20">
        <CardContent className="flex items-center gap-3 p-3">
          <Shield className="h-4 w-4 text-muted-foreground" />
          <p className="text-xs text-muted-foreground">Ledger entries are append-only and cannot be edited or deleted.</p>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="p-0">
          <DataTable columns={columns} data={data?.items || []} keyExtractor={(e) => e.id}
            isEmpty={!data?.items?.length} emptyMessage="No ledger entries found" />
          {data && data.total > PAGE_SIZE && (
            <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />
          )}
        </CardContent>
      </Card>
    </div>
  );
}
