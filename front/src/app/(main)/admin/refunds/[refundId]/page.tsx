'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { DetailPageSkeleton, EmptyState, ErrorState } from '@/components/shared/states';
import {
  useRefundQuery,
  useApproveRefundMutation,
  useRejectRefundMutation,
  useProcessRefundMutation,
  useRetryRefundMutation,
  useCancelRefundMutation,
  useMarkManualReviewMutation,
} from '@/features/refunds/api/refunds.queries';
import type { RefundRequestDto } from '@/features/refunds/types/refund.types';
import { ArrowLeft, Loader2 } from 'lucide-react';
import { useT } from '@/shared/i18n/LanguageProvider';

const statusColors: Record<string, string> = {
  Requested: 'text-gray-600',
  PendingReview: 'text-yellow-600',
  Approved: 'text-blue-600',
  Rejected: 'text-red-600',
  Cancelled: 'text-gray-400',
  HoldPending: 'text-purple-600',
  HoldCreated: 'text-purple-600',
  Processing: 'text-indigo-600',
  GatewaySubmitted: 'text-indigo-600',
  GatewaySucceeded: 'text-green-600',
  GatewayFailed: 'text-red-600',
  WalletDebitPending: 'text-teal-600',
  WalletDebited: 'text-teal-600',
  Completed: 'text-green-700 font-semibold',
  Failed: 'text-red-700 font-semibold',
  RequiresManualReview: 'text-orange-600',
};

function ConfirmModal({ open, title, message, onConfirm, onCancel, loading, children }: {
  open: boolean; title: string; message: string;
  onConfirm: () => void; onCancel: () => void; loading?: boolean;
  children?: React.ReactNode;
}) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
        <h3 className="text-lg font-semibold">{title}</h3>
        <p className="mt-2 text-sm text-gray-600">{message}</p>
        {children}
        <div className="mt-4 flex justify-end gap-2">
          <Button variant="outline" onClick={onCancel} disabled={loading}>Cancel</Button>
          <Button onClick={onConfirm} disabled={loading}>
            {loading && <Loader2 className="mr-1 h-4 w-4 animate-spin" />}
            Confirm
          </Button>
        </div>
      </div>
    </div>
  );
}

export default function RefundDetailPage() {
  return (
    <RoutePermissionGuard permissions={['payment.refund.view_admin']}>
      <RefundDetailContent />
    </RoutePermissionGuard>
  );
}

function RefundDetailContent() {
  const { refundId } = useParams<{ refundId: string }>();
  const router = useRouter();
  const { t } = useT();
  const { data: refund, isLoading, isError, error, refetch } = useRefundQuery(refundId);

  const [confirm, setConfirm] = useState<{ action: string; message: string } | null>(null);
  const [rejectReason, setRejectReason] = useState('');
  const [manualReason, setManualReason] = useState('');

  const approveMutation = useApproveRefundMutation();
  const rejectMutation = useRejectRefundMutation();
  const processMutation = useProcessRefundMutation();
  const retryMutation = useRetryRefundMutation();
  const cancelMutation = useCancelRefundMutation();
  const manualReviewMutation = useMarkManualReviewMutation();

  if (isLoading) {
    return <DetailPageSkeleton cards={3} />;
  }

  if (isError) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" onClick={() => router.push('/admin/refunds')}><ArrowLeft className="mr-1 h-4 w-4" />Back</Button>
        <Card><CardContent className="py-10">
          <ErrorState error={(error as Error)?.message} onRetry={() => refetch()} />
        </CardContent></Card>
      </div>
    );
  }

  if (!refund) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" onClick={() => router.push('/admin/refunds')}><ArrowLeft className="mr-1 h-4 w-4" />Back</Button>
        <Card><CardContent className="py-10">
          <EmptyState
            icon="search"
            title={t('refund.notFoundTitle')}
            description={t('refund.notFound')}
            action={{ label: 'Back to Refunds', onClick: () => router.push('/admin/refunds') }}
          />
        </CardContent></Card>
      </div>
    );
  }

  const isPending = ['Requested', 'PendingReview'].includes(refund.status);
  const confirmDisabled = (loading: boolean) =>
    loading || (confirm?.action === 'reject' && !rejectReason.trim());
  const canApprove = refund.status === 'PendingReview';
  const canProcess = refund.status === 'Approved' || refund.status === 'HoldCreated';
  const canRetry = refund.status === 'GatewayFailed' || refund.status === 'Failed';
  const isTerminal = ['Completed', 'Rejected', 'Cancelled'].includes(refund.status);

  const executeConfirm = () => {
    if (!confirm || !refund) return;
    switch (confirm.action) {
      case 'approve': approveMutation.mutate({ refundId: refund.id }); break;
      case 'reject': rejectMutation.mutate({ refundId: refund.id, request: { reason: rejectReason } }); break;
      case 'process': processMutation.mutate(refund.id); break;
      case 'retry': retryMutation.mutate(refund.id); break;
      case 'cancel': cancelMutation.mutate(refund.id); break;
      case 'manual': manualReviewMutation.mutate({ refundId: refund.id, reason: manualReason }); break;
    }
    setConfirm(null);
    setRejectReason('');
    setManualReason('');
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="sm" onClick={() => router.push('/admin/refunds')}>
            <ArrowLeft className="mr-1 h-4 w-4" />Back
          </Button>
          <h1 className="text-2xl font-bold tracking-tight">Refund Detail</h1>
        </div>
        <Button variant="outline" size="sm" onClick={() => refetch()}>Refresh</Button>
      </div>

      {/* Status & Actions */}
      <Card>
        <CardHeader className="pb-2">
          <div className="flex items-center justify-between">
            <CardTitle className="text-lg">Refund Request</CardTitle>
            <span className={`text-sm font-medium ${statusColors[refund.status]}`}>{refund.status}</span>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div><span className="text-muted-foreground">Amount</span>
              <div className="font-mono font-medium">{refund.amount.toLocaleString()} {refund.currency}</div>
            </div>
            <div><span className="text-muted-foreground">Gateway</span>
              <div>{refund.provider_code}</div>
            </div>
            <div><span className="text-muted-foreground">Created</span>
              <div>{new Date(refund.created_at).toLocaleDateString('fa-IR')}</div>
            </div>
            <div><span className="text-muted-foreground">Transaction</span>
              <div className="font-mono text-xs">{refund.payment_transaction_id.slice(0, 12)}…</div>
            </div>
          </div>

          <div className="text-sm"><span className="text-muted-foreground">Reason: </span>{refund.reason}</div>

          {/* Hold info */}
          {refund.wallet_hold_id && (
            <div className="rounded-md bg-purple-50 p-3 text-sm">
              <span className="font-medium text-purple-700">Wallet Hold: </span>
              <span className="font-mono text-xs text-purple-600">{refund.wallet_hold_id}</span>
              <span className="ml-2 text-purple-600">({refund.amount.toLocaleString()} {refund.currency} reserved)</span>
            </div>
          )}

          {/* Gateway info */}
          {refund.gateway_refund_id && (
            <div className="rounded-md bg-green-50 p-3 text-sm">
              <span className="font-medium text-green-700">Gateway Refund ID: </span>
              <span className="font-mono text-xs text-green-600">{refund.gateway_refund_id}</span>
            </div>
          )}

          {/* Failure info */}
          {refund.failure_code && (
            <div className="rounded-md bg-red-50 p-3 text-sm">
              <span className="font-medium text-red-700">Failure: </span>
              <span className="text-red-600">{refund.failure_code}</span>
              {refund.failure_message && <div className="mt-1 text-red-500">{refund.failure_message}</div>}
            </div>
          )}

          {/* Action buttons */}
          {!isTerminal && (
            <div className="flex flex-wrap gap-2 pt-2 border-t">
              {canApprove && (
                <>
                  <Button size="sm" onClick={() => setConfirm({ action: 'approve', message: `Approve refund of ${refund.amount.toLocaleString()} ${refund.currency}?` })} disabled={approveMutation.isPending}>
                    {approveMutation.isPending && <Loader2 className="mr-1 h-3 w-3 animate-spin" />}Approve
                  </Button>
                  <Button size="sm" variant="destructive" onClick={() => {
                    setRejectReason('');
                    setConfirm({ action: 'reject', message: `Reject refund of ${refund.amount.toLocaleString()} ${refund.currency}?` });
                  }}>Reject</Button>
                </>
              )}
              {canProcess && (
                <Button size="sm" variant="default" className="bg-indigo-600 hover:bg-indigo-700" onClick={() => setConfirm({
                  action: 'process',
                  message: `Process refund: ${refund.amount.toLocaleString()} ${refund.currency} will be reserved from wallet, then sent to gateway. Continue?`
                })} disabled={processMutation.isPending}>
                  {processMutation.isPending && <Loader2 className="mr-1 h-3 w-3 animate-spin" />}Process
                </Button>
              )}
              {canRetry && (
                <Button size="sm" variant="outline" onClick={() => setConfirm({ action: 'retry', message: 'Retry the gateway refund call?' })} disabled={retryMutation.isPending}>
                  {retryMutation.isPending && <Loader2 className="mr-1 h-3 w-3 animate-spin" />}Retry
                </Button>
              )}
              {isPending && (
                <Button size="sm" variant="outline" onClick={() => setConfirm({ action: 'cancel', message: 'Cancel this refund request?' })} disabled={cancelMutation.isPending}>Cancel</Button>
              )}
              <Button size="sm" variant="ghost" onClick={() => {
                setManualReason('');
                setConfirm({ action: 'manual', message: 'Mark refund for manual review?' });
              }}>Mark for Review</Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Gateway Attempts */}
      {refund.attempts && refund.attempts.length > 0 && (
        <Card>
          <CardHeader><CardTitle className="text-base">Gateway Attempts</CardTitle></CardHeader>
          <CardContent>
            <div className="space-y-2">
              {refund.attempts.map((a) => (
                <div key={a.id} className="flex items-center justify-between rounded-md border p-3 text-sm">
                  <div className="flex items-center gap-3">
                    <span className="font-mono text-xs text-muted-foreground">#{a.attempt_no}</span>
                    <span className={statusColors[a.status]}>{a.status}</span>
                    {a.gateway_refund_id && <span className="font-mono text-xs">{a.gateway_refund_id.slice(0, 16)}…</span>}
                  </div>
                  <div className="text-xs text-muted-foreground">
                    {a.error_code && <span className="text-red-500 mr-2">{a.error_code}</span>}
                    {a.finished_at ? new Date(a.finished_at).toLocaleString('fa-IR') : a.started_at ? `${new Date(a.started_at).toLocaleString('fa-IR')} (ongoing)` : ''}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Details */}
      <Card>
        <CardHeader><CardTitle className="text-base">Details</CardTitle></CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-2 text-sm">
            <div><span className="text-muted-foreground">Refund ID:</span> <span className="font-mono text-xs">{refund.id}</span></div>
            <div><span className="text-muted-foreground">Transaction ID:</span> <span className="font-mono text-xs">{refund.payment_transaction_id}</span></div>
            <div><span className="text-muted-foreground">Gateway Config:</span> <span className="font-mono text-xs">{refund.gateway_config_id}</span></div>
            <div><span className="text-muted-foreground">Wallet:</span> <span className="font-mono text-xs">{refund.wallet_id || '—'}</span></div>
            <div><span className="text-muted-foreground">Hold ID:</span> <span className="font-mono text-xs">{refund.wallet_hold_id || '—'}</span></div>
            <div><span className="text-muted-foreground">Approved By:</span> {refund.approved_by_user_id || '—'}</div>
            <div><span className="text-muted-foreground">Approved At:</span> {refund.approved_at ? new Date(refund.approved_at).toLocaleString('fa-IR') : '—'}</div>
            <div><span className="text-muted-foreground">Completed At:</span> {refund.completed_at ? new Date(refund.completed_at).toLocaleString('fa-IR') : '—'}</div>
          </div>
        </CardContent>
      </Card>

      {/* Confirm Modal */}
      <ConfirmModal
        open={!!confirm}
        title="Confirm Action"
        message={confirm?.message || ''}
        loading={approveMutation.isPending || rejectMutation.isPending || processMutation.isPending || retryMutation.isPending}
        onConfirm={executeConfirm}
        onCancel={() => { setConfirm(null); setRejectReason(''); setManualReason(''); }}
      >
        {(confirm?.action === 'reject') && (
          <div className="mt-3">
            <label className="block text-sm font-medium mb-1">Rejection reason (required):</label>
            <input
              type="text"
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
              className="w-full rounded-md border px-3 py-2 text-sm"
              placeholder="Enter reason for rejection"
            />
          </div>
        )}
        {(confirm?.action === 'manual') && (
          <div className="mt-3">
            <label className="block text-sm font-medium mb-1">Manual review reason:</label>
            <input
              type="text"
              value={manualReason}
              onChange={(e) => setManualReason(e.target.value)}
              className="w-full rounded-md border px-3 py-2 text-sm"
              placeholder="Enter reason for manual review"
            />
          </div>
        )}
      </ConfirmModal>
    </div>
  );
}
