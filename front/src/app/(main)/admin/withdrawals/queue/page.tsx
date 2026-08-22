'use client';

import { Suspense, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable, DataTableColumn, Pagination } from '@/components/shared/data-table';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { FilterBar, RefreshButton } from '@/components/shared/filters';
import { MaskedCardNumber, MaskedIban } from '@/components/shared/masked-value';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { getWithdrawalQueue, type WithdrawalItem } from '@/api/withdrawals';
import { cn } from '@/lib/utils';

const PAGE_SIZE = 20;
const STATUSES = ['pending_review', 'more_info_required', 'approved', 'processing_payout', 'failed', 'paid', 'rejected'];

export default function WithdrawalQueuePage() {
  return (
    <RoutePermissionGuard permissions={['payment.withdrawal.view_admin']}>
      <Suspense fallback={<div className="p-8 text-center text-muted-foreground">Loading...</div>}>
        <WithdrawalQueueContent />
      </Suspense>
    </RoutePermissionGuard>
  );
}

function WithdrawalQueueContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const initialStatus = searchParams.get('status') || 'pending_review';
  const [activeStatus, setActiveStatus] = useState(initialStatus);
  const [skip, setSkip] = useState(0);

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['withdrawal-queue', { status: activeStatus, skip }],
    queryFn: () => getWithdrawalQueue({ status: activeStatus, skip, take: PAGE_SIZE }),
  });

  const columns: DataTableColumn<WithdrawalItem>[] = [
    { key: 'id', header: 'ID', render: (w) => <Link href={`/admin/withdrawals/${w.id}`} className="font-mono text-xs text-primary hover:underline">{w.id.slice(0, 12)}…</Link> },
    { key: 'amount', header: 'Amount', render: (w) => <MoneyAmount amount={w.amount} currency={w.currency} className="text-xs" /> },
    { key: 'fee', header: 'Fee', render: (w) => <MoneyAmount amount={w.feeAmount} currency={w.currency} className="text-xs" /> },
    { key: 'net', header: 'Net', render: (w) => <MoneyAmount amount={w.netAmount} currency={w.currency} className="text-xs font-medium" /> },
    { key: 'payout', header: 'Payout', render: (w) => w.cardMasked ? <MaskedCardNumber value={w.cardMasked} /> : w.ibanMasked ? <MaskedIban value={w.ibanMasked} /> : <span className="text-xs">—</span> },
    { key: 'status', header: 'Status', render: (w) => <StatusBadge status={w.status} /> },
    { key: 'date', header: 'Date', render: (w) => <span className="text-xs">{new Date(w.createdAt).toLocaleDateString()}</span> },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Withdrawal Queue</h1>
          <p className="text-sm text-muted-foreground">Admin withdrawal operations queue</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <div className="flex gap-1 overflow-x-auto pb-2">
        {STATUSES.map((s) => (
          <button key={s} onClick={() => { setActiveStatus(s); setSkip(0); }}
            className={cn('shrink-0 rounded-full px-3 py-1.5 text-xs font-medium transition-colors capitalize',
              activeStatus === s ? 'bg-primary text-primary-foreground' : 'border hover:bg-accent')}>
            {s.replace(/_/g, ' ')}
            {data?.countsByStatus?.[s] != null && <span className="ml-1 opacity-70">({data.countsByStatus[s]})</span>}
          </button>
        ))}
      </div>

      <FilterBar><div className="text-xs text-muted-foreground">Showing: <strong className="capitalize">{activeStatus.replace(/_/g, ' ')}</strong></div></FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(w) => w.id} isLoading={isLoading} isEmpty={!data?.items?.length} emptyMessage={`No ${activeStatus.replace(/_/g, ' ')} withdrawals`} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} onRowClick={(w) => router.push(`/admin/withdrawals/${w.id}`)} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>
    </div>
  );
}
