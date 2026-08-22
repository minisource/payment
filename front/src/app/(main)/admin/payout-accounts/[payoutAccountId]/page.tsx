'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { DataTable } from '@/components/shared/data-table';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { ConfirmDialog } from '@/components/shared/components';
import { MaskedCardNumber, MaskedIban, MaskedAccountNumber } from '@/components/shared/masked-value';
import { PermissionGuard } from '@/components/shared/permission-guards';
import { toast } from 'sonner';
import { getPayoutAccount, verifyPayoutAccount, rejectPayoutAccount, disablePayoutAccount } from '@/api/payout-accounts';
import { CheckCircle, XCircle, Ban } from 'lucide-react';

export default function PayoutAccountDetailPage() {
  const { payoutAccountId } = useParams<{ payoutAccountId: string }>();
  const queryClient = useQueryClient();
  const [action, setAction] = useState<'verify' | 'reject' | 'disable' | null>(null);
  const [reason, setReason] = useState('');

  const { data: account, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['payout-account', payoutAccountId],
    queryFn: () => getPayoutAccount(payoutAccountId),
  });

  const actionMutation = useMutation({
    mutationFn: (req: { action: 'verify' | 'reject' | 'disable'; reason?: string }) =>
      req.action === 'verify' ? verifyPayoutAccount(payoutAccountId) : req.action === 'reject' ? rejectPayoutAccount(payoutAccountId, req.reason!) : disablePayoutAccount(payoutAccountId, req.reason!),
    onSuccess: () => { toast.success('Action completed'); queryClient.invalidateQueries({ queryKey: ['payout-account', payoutAccountId] }); setAction(null); setReason(''); },
    onError: (err: Error) => toast.error(err.message),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !account) return <ErrorState error={(error as Error)?.message || 'Not found'} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <Link href="/admin/payout-accounts" className="text-sm text-muted-foreground hover:text-foreground">← Payout Accounts</Link>
          <div className="mt-1 flex items-center gap-3"><h1 className="text-xl font-bold">{account.holderName}</h1><StatusBadge status={account.status} /></div>
        </div>
        <div className="flex gap-2">
          <PermissionGuard permission="payment.payout_account.verify_admin">
            {account.status === 'pending_verification' && <Button variant="outline" size="sm" onClick={() => setAction('verify')} className="text-green-600"><CheckCircle className="mr-1 h-3.5 w-3.5" /> Verify</Button>}
          </PermissionGuard>
          <PermissionGuard permission="payment.payout_account.reject_admin">
            {account.status === 'pending_verification' && <Button variant="outline" size="sm" onClick={() => setAction('reject')} className="text-red-600"><XCircle className="mr-1 h-3.5 w-3.5" /> Reject</Button>}
          </PermissionGuard>
          <PermissionGuard permission="payment.payout_account.disable_admin">
            {account.status !== 'disabled' && account.status !== 'deleted' && <Button variant="outline" size="sm" onClick={() => setAction('disable')} className="text-red-600"><Ban className="mr-1 h-3.5 w-3.5" /> Disable</Button>}
          </PermissionGuard>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Account Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">ID</span><span className="font-mono text-xs">{account.id}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Owner</span><span className="text-xs">{account.ownerType}: {account.ownerId}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Type</span><span className="text-xs capitalize">{account.accountType.replace(/_/g, ' ')}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Holder</span><span className="text-xs">{account.holderName}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Bank</span><span className="text-xs">{account.bankName || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Currency</span><span className="text-xs font-mono">{account.currency}</span></div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Account Numbers</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            {account.cardNumberMasked && <div className="flex justify-between"><span className="text-muted-foreground">Card</span><MaskedCardNumber value={account.cardNumberMasked} /></div>}
            {account.ibanMasked && <div className="flex justify-between"><span className="text-muted-foreground">IBAN</span><MaskedIban value={account.ibanMasked} /></div>}
            {account.accountNumberMasked && <div className="flex justify-between"><span className="text-muted-foreground">Account</span><MaskedAccountNumber value={account.accountNumberMasked} /></div>}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><CardTitle className="text-sm">Verification Status</CardTitle></CardHeader>
        <CardContent className="space-y-2 text-sm">
          <div className="flex justify-between"><span className="text-muted-foreground">Status</span><StatusBadge status={account.status} /></div>
          {account.verifiedAt && <div className="flex justify-between"><span className="text-muted-foreground">Verified</span><span className="text-xs">{new Date(account.verifiedAt).toLocaleString()}</span></div>}
          {account.verifiedByUserId && <div className="flex justify-between"><span className="text-muted-foreground">Verified By</span><span className="font-mono text-xs">{account.verifiedByUserId}</span></div>}
          {account.rejectedAt && <div className="flex justify-between"><span className="text-muted-foreground">Rejected</span><span className="text-xs">{new Date(account.rejectedAt).toLocaleString()}</span></div>}
          {account.rejectionReason && <div className="flex justify-between"><span className="text-muted-foreground">Reason</span><span className="text-xs text-red-600">{account.rejectionReason}</span></div>}
          {account.disabledAt && <div className="flex justify-between"><span className="text-muted-foreground">Disabled</span><span className="text-xs">{new Date(account.disabledAt).toLocaleString()}</span></div>}
          {account.disableReason && <div className="flex justify-between"><span className="text-muted-foreground">Reason</span><span className="text-xs text-red-600">{account.disableReason}</span></div>}
        </CardContent>
      </Card>

      {account.relatedWithdrawals?.length > 0 && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Related Withdrawals</CardTitle></CardHeader>
          <CardContent className="p-0">
            <DataTable columns={[
              { key: 'id' as const, header: 'ID', render: (w: any) => <span className="font-mono text-xs">{w.id.slice(0, 12)}…</span> },
              { key: 'amount' as const, header: 'Amount', render: (w: any) => <MoneyAmount amount={w.amount} currency={w.currency} className="text-xs" /> },
              { key: 'status' as const, header: 'Status', render: (w: any) => <StatusBadge status={w.status} /> },
              { key: 'date' as const, header: 'Date', render: (w: any) => <span className="text-xs">{new Date(w.createdAt).toLocaleDateString()}</span> },
            ]} data={account.relatedWithdrawals} keyExtractor={(w) => w.id} compact />
          </CardContent>
        </Card>
      )}

      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={() => actionMutation.mutate({ action: action!, reason })} confirmDisabled={action !== 'verify' && !reason}
          title={`${action === 'verify' ? 'Verify' : action === 'reject' ? 'Reject' : 'Disable'} Payout Account`}
          description={action !== 'verify' ? <div className="mt-3"><label className="text-xs font-medium">Reason *</label><input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div> : undefined}
          confirmLabel={action === 'verify' ? 'Verify' : action === 'reject' ? 'Reject' : 'Disable'} destructive={action !== 'verify'} />
      )}
    </div>
  );
}
