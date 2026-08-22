'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { SearchInput, FilterBar, RefreshButton } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { toast } from 'sonner';
import { usePaymentLinksQuery, usePaymentLinkToggleMutation } from '@/features/payment-links/api/payment-links.queries';
import { PaymentLinksDataTable } from '@/features/payment-links/components/PaymentLinksDataTable';

const PAGE_SIZE = 20;

export default function PaymentLinksPage() {
  return (
    <RoutePermissionGuard permissions={['payment.payment_link.view_admin']}>
      <PaymentLinksContent />
    </RoutePermissionGuard>
  );
}

function PaymentLinksContent() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [skip, setSkip] = useState(0);
  const [statusFilter, setStatusFilter] = useState('');
  const [actionLink, setActionLink] = useState<{ id: string; action: 'pause' | 'resume' | 'disable' } | null>(null);

  const { data, isLoading, isError, error, refetch } = usePaymentLinksQuery({
    query: search || undefined,
    skip,
    take: PAGE_SIZE,
    status: statusFilter || undefined,
  });

  const toggleMutation = usePaymentLinkToggleMutation();

  const handleAction = async () => {
    if (!actionLink) return;
    try {
      await toggleMutation.mutateAsync(actionLink);
      toast.success('Action completed');
      setActionLink(null);
    } catch (err: any) {
      toast.error(err.message || 'Action failed');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Payment Links</h1>
          <p className="text-sm text-muted-foreground">Public payment links</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={(v) => { setSearch(v); setSkip(0); }} placeholder="Search links..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option>
          <option value="active">Active</option>
          <option value="paused">Paused</option>
          <option value="disabled">Disabled</option>
          <option value="expired">Expired</option>
        </select>
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <PaymentLinksDataTable
            data={data?.items || []}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No payment links found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(p) => router.push(`/admin/payment-links/${p.id}`)}
            skip={skip}
            take={PAGE_SIZE}
            total={data?.total}
            onPaginationChange={setSkip}
          />
        </CardContent>
      </Card>

      {actionLink && (
        <ConfirmDialog
          open
          onClose={() => setActionLink(null)}
          onConfirm={handleAction}
          title={`${actionLink.action === 'pause' ? 'Pause' : actionLink.action === 'resume' ? 'Resume' : 'Disable'} Payment Link`}
          confirmLabel={actionLink.action === 'pause' ? 'Pause' : actionLink.action === 'resume' ? 'Resume' : 'Disable'}
          destructive={actionLink.action === 'disable'}
          confirmDisabled={toggleMutation.isPending}
        />
      )}
    </div>
  );
}
