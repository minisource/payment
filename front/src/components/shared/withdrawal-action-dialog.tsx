'use client';

import { useState } from 'react';
import { ConfirmDialog } from '@/components/shared/components';
import { MoneyAmount } from '@/components/shared/money-amount';
import { MaskedCardNumber, MaskedIban } from '@/components/shared/masked-value';
import { toast } from 'sonner';
import {
  approveWithdrawal, rejectWithdrawal, requestWithdrawalMoreInfo,
  markWithdrawalProcessing, markWithdrawalPaid, markWithdrawalFailed,
  type WithdrawalDetail, type WithdrawalActionResult,
} from '@/api/withdrawals';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { CheckCircle, XCircle, HelpCircle, Play, Banknote, AlertTriangle } from 'lucide-react';

interface WithdrawalActionDialogProps {
  withdrawal: WithdrawalDetail;
  action: 'approve' | 'reject' | 'request-more-info' | 'mark-processing' | 'mark-paid' | 'mark-failed';
  open: boolean;
  onClose: () => void;
}

export function WithdrawalActionDialog({ withdrawal, action, open, onClose }: WithdrawalActionDialogProps) {
  const queryClient = useQueryClient();
  const [reason, setReason] = useState('');
  const [note, setNote] = useState('');
  const [releaseFunds, setReleaseFunds] = useState(true);
  const [bankTrackingNumber, setBankTrackingNumber] = useState('');
  const [bankReferenceId, setBankReferenceId] = useState('');
  const [paidAt, setPaidAt] = useState(new Date().toISOString().slice(0, 16));

  const mutation = useMutation({
    mutationFn: async (): Promise<WithdrawalActionResult> => {
      switch (action) {
        case 'approve': return approveWithdrawal(withdrawal.id, { note: note || undefined });
        case 'reject': return rejectWithdrawal(withdrawal.id, { reason, releaseFunds });
        case 'request-more-info': return requestWithdrawalMoreInfo(withdrawal.id, { reason });
        case 'mark-processing': return markWithdrawalProcessing(withdrawal.id, { note: note || undefined });
        case 'mark-paid': return markWithdrawalPaid(withdrawal.id, { bankTrackingNumber, bankReferenceId: bankReferenceId || undefined, note: note || undefined, paidAt: paidAt ? new Date(paidAt).toISOString() : undefined });
        case 'mark-failed': return markWithdrawalFailed(withdrawal.id, { reason, releaseFunds });
      }
    },
    onSuccess: (res) => {
      if (res.approvalRequestId) {
        toast.success('Approval request created', { description: `Request ID: ${res.approvalRequestId}` });
      } else {
        toast.success(res.message || `${action} completed`);
      }
      onClose();
      queryClient.invalidateQueries({ queryKey: ['withdrawal', withdrawal.id] });
      queryClient.invalidateQueries({ queryKey: ['withdrawals'] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const configs = {
    approve: { title: 'Approve Withdrawal', icon: CheckCircle, destructive: false, confirmLabel: 'Approve', permission: 'payment.withdrawal.approve', needsReason: false, needsBankTracking: false },
    reject: { title: 'Reject Withdrawal', icon: XCircle, destructive: true, confirmLabel: 'Reject', permission: 'payment.withdrawal.reject', needsReason: true, needsBankTracking: false },
    'request-more-info': { title: 'Request More Info', icon: HelpCircle, destructive: false, confirmLabel: 'Request Info', permission: 'payment.withdrawal.request_more_info', needsReason: true, needsBankTracking: false },
    'mark-processing': { title: 'Mark Processing', icon: Play, destructive: false, confirmLabel: 'Mark Processing', permission: 'payment.withdrawal.mark_processing', needsReason: false, needsBankTracking: false },
    'mark-paid': { title: 'Mark as Paid', icon: Banknote, destructive: false, confirmLabel: 'Mark Paid', permission: 'payment.withdrawal.mark_paid', needsReason: false, needsBankTracking: true },
    'mark-failed': { title: 'Mark as Failed', icon: AlertTriangle, destructive: true, confirmLabel: 'Mark Failed', permission: 'payment.withdrawal.mark_failed', needsReason: true, needsBankTracking: false },
  };
  const cfg = configs[action];
  const canSubmit = mutation.isPending ? false : cfg.needsReason ? !!reason : cfg.needsBankTracking ? !!bankTrackingNumber : true;

  return (
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={() => mutation.mutate()}
      title={cfg.title}
      description={
        <div className="mt-3 space-y-3">
          <div className="rounded-md border p-3 space-y-1">
            <div className="flex justify-between text-xs"><span className="text-muted-foreground">Amount</span><MoneyAmount amount={withdrawal.amount} currency={withdrawal.currency} /></div>
            <div className="flex justify-between text-xs"><span className="text-muted-foreground">Fee</span><MoneyAmount amount={withdrawal.feeAmount} currency={withdrawal.currency} /></div>
            <div className="flex justify-between text-xs"><span className="text-muted-foreground">Net</span><MoneyAmount amount={withdrawal.netAmount} currency={withdrawal.currency} /></div>
          </div>
          <div className="text-xs text-muted-foreground">
            Payout: {withdrawal.cardMasked ? <MaskedCardNumber value={withdrawal.cardMasked} /> : withdrawal.ibanMasked ? <MaskedIban value={withdrawal.ibanMasked} /> : '—'}
          </div>
          {cfg.needsReason && (
            <div>
              <label className="text-xs font-medium">Reason *</label>
              <input value={reason} onChange={(e) => setReason(e.target.value)} placeholder="Enter reason" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
          )}
          {cfg.needsBankTracking && (
            <div>
              <label className="text-xs font-medium">Bank Tracking Number *</label>
              <input value={bankTrackingNumber} onChange={(e) => setBankTrackingNumber(e.target.value)} placeholder="e.g. TRK123" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
              <input value={bankReferenceId} onChange={(e) => setBankReferenceId(e.target.value)} placeholder="Reference ID (optional)" className="mt-2 w-full rounded-md border px-3 py-2 text-sm" />
              <input type="datetime-local" value={paidAt} onChange={(e) => setPaidAt(e.target.value)} className="mt-2 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
          )}
          {!cfg.needsReason && !cfg.needsBankTracking && (
            <div>
              <label className="text-xs font-medium">Note (optional)</label>
              <input value={note} onChange={(e) => setNote(e.target.value)} placeholder="Optional note" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
          )}
          {(action === 'reject' || action === 'mark-failed') && (
            <div className="flex items-center gap-2">
              <input type="checkbox" checked={releaseFunds} onChange={(e) => setReleaseFunds(e.target.checked)} id="releaseFunds" />
              <label htmlFor="releaseFunds" className="text-xs">Release locked funds back to wallet</label>
            </div>
          )}
        </div>
      }
      confirmLabel={mutation.isPending ? 'Processing…' : cfg.confirmLabel}
      destructive={cfg.destructive}
      confirmDisabled={!canSubmit}
    />
  );
}
