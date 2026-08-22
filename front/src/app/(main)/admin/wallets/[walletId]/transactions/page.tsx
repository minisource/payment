'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable, DataTableColumn, Pagination } from '@/components/shared/data-table';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { FilterBar } from '@/components/shared/filters';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { listWalletTransactions, type WalletTransaction } from '@/api/wallets';

const PAGE_SIZE = 25;

export default function WalletTransactionsPage() {
  const { walletId } = useParams<{ walletId: string }>();
  const [skip, setSkip] = useState(0);
  const [typeFilter, setTypeFilter] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['wallet-transactions', walletId, skip, typeFilter],
    queryFn: () => listWalletTransactions(walletId, { skip, take: PAGE_SIZE, type: typeFilter || undefined }),
  });

  if (isLoading) return <DetailPageSkeleton cards={2} />;
  if (isError) return <ErrorState error={(error as Error)?.message} onRetry={() => refetch()} />;

  const columns: DataTableColumn<WalletTransaction>[] = [
    { key: 'id', header: 'ID', render: (t) => <span className="font-mono text-xs">{t.id.slice(0, 12)}…</span> },
    { key: 'type', header: 'Type', render: (t) => <span className="text-xs capitalize">{t.type.replace(/_/g, ' ')}</span> },
    { key: 'status', header: 'Status', render: (t) => <StatusBadge status={t.status} /> },
    { key: 'amount', header: 'Amount', render: (t) => <MoneyAmount amount={t.amount} currency={t.currency} className="text-xs" /> },
    { key: 'reference', header: 'Reference', render: (t) => t.referenceType ? <span className="text-xs">{t.referenceType}: {t.referenceId?.slice(0, 12)}</span> : <span className="text-xs text-muted-foreground">—</span> },
    { key: 'createdAt', header: 'Date', render: (t) => <span className="text-xs">{new Date(t.createdAt).toLocaleString()}</span> },
  ];

  return (
    <div className="space-y-6">
      <div>
        <Link href={`/admin/wallets/${walletId}`} className="text-sm text-muted-foreground hover:text-foreground">← Wallet {walletId.slice(0, 12)}…</Link>
        <h1 className="mt-1 text-2xl font-bold tracking-tight">Wallet Transactions</h1>
        <p className="text-sm text-muted-foreground">All transactions for this wallet</p>
      </div>

      <FilterBar>
        <select value={typeFilter} onChange={(e) => { setTypeFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Types</option>
          <option value="wallet_credit">Wallet Credit</option>
          <option value="wallet_debit">Wallet Debit</option>
          <option value="admin_credit">Admin Credit</option>
          <option value="admin_debit">Admin Debit</option>
          <option value="payment_credit">Payment Credit</option>
          <option value="withdrawal_lock">Withdrawal Lock</option>
          <option value="withdrawal_release">Withdrawal Release</option>
          <option value="withdrawal_capture">Withdrawal Capture</option>
          <option value="refund">Refund</option>
          <option value="reversal">Reversal</option>
        </select>
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <DataTable columns={columns} data={data?.items || []} keyExtractor={(t) => t.id}
            isEmpty={!data?.items?.length} emptyMessage="No transactions found" />
          {data && data.total > PAGE_SIZE && (
            <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />
          )}
        </CardContent>
      </Card>
    </div>
  );
}
