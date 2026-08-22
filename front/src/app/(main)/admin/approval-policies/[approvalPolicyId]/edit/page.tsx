'use client';

import { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useQuery, useMutation } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Loading, ErrorState } from '@/components/shared/states';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { toast } from 'sonner';
import { getApprovalPolicy, updateApprovalPolicy, type ApprovalPolicyUpdateRequest } from '@/api/approval-policies';

export default function EditApprovalPolicyPage() {
  return (
    <RoutePermissionGuard permissions={['payment.admin_approval.policy.update']}>
      <EditPolicyContent />
    </RoutePermissionGuard>
  );
}

function EditPolicyContent() {
  const { approvalPolicyId } = useParams<{ approvalPolicyId: string }>();
  const router = useRouter();
  const [form, setForm] = useState<ApprovalPolicyUpdateRequest>({});

  const { data: policy, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['approval-policy', approvalPolicyId],
    queryFn: () => getApprovalPolicy(approvalPolicyId),
  });

  useEffect(() => {
    if (policy) {
      setForm({
        name: policy.name,
        description: policy.description || '',
        operationType: policy.operationType,
        requiredApprovals: policy.requiredApprovals,
        requireDifferentUser: policy.requireDifferentUser,
        amountThreshold: policy.amountThreshold || '',
        currency: policy.currency || '',
        expiresAfterMinutes: policy.expiresAfterMinutes || '',
      } as any);
    }
  }, [policy]);

  const updateMutation = useMutation({
    mutationFn: () => updateApprovalPolicy(approvalPolicyId, form),
    onSuccess: () => { toast.success('Policy updated'); router.push(`/admin/approval-policies/${approvalPolicyId}`); },
    onError: (err: Error) => toast.error(err.message),
  });

  const handleSubmit = () => {
    if (form.requiredApprovals != null && (form.requiredApprovals as number) < 1) { toast.error('At least 1 approval required'); return; }
    updateMutation.mutate();
  };

  const setField = (k: string, v: any) => setForm((p) => ({ ...p, [k]: v }));

  if (isLoading) return <Loading />;
  if (isError || !policy) return <ErrorState error={(error as Error)?.message || 'Not found'} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6 max-w-2xl">
      <div>
        <Link href={`/admin/approval-policies/${approvalPolicyId}`} className="text-sm text-muted-foreground hover:text-foreground">← Policy</Link>
        <h1 className="text-2xl font-bold">Edit Approval Policy</h1>
      </div>

      <Card>
        <CardHeader><CardTitle className="text-sm">Policy Details</CardTitle></CardHeader>
        <CardContent className="space-y-4">
          <div>
            <label className="text-xs font-medium">Name</label>
            <input value={(form.name as string) || ''} onChange={(e) => setField('name', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="text-xs font-medium">Description</label>
            <input value={(form.description as string) || ''} onChange={(e) => setField('description', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="text-xs font-medium">Operation Type</label>
            <select value={(form.operationType as string) || ''} onChange={(e) => setField('operationType', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm">
              <option value="">Select...</option>
              <option value="wallet_adjustment">Wallet Adjustment</option>
              <option value="withdrawal_approve">Withdrawal Approve</option>
              <option value="withdrawal_mark_paid">Withdrawal Mark Paid</option>
              <option value="reconciliation_run">Reconciliation Run</option>
              <option value="gateway_config_change">Gateway Config Change</option>
              <option value="admin_sensitive_action">Admin Sensitive Action</option>
            </select>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="text-xs font-medium">Required Approvals</label>
              <input type="number" min={1} value={(form.requiredApprovals as number) || 1} onChange={(e) => setField('requiredApprovals', parseInt(e.target.value) || 1)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
            <div>
              <label className="text-xs font-medium">Different User Required</label>
              <select value={(form.requireDifferentUser as boolean) ? 'true' : 'false'} onChange={(e) => setField('requireDifferentUser', e.target.value === 'true')} className="mt-1 w-full rounded-md border px-3 py-2 text-sm">
                <option value="true">Yes</option><option value="false">No</option>
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="text-xs font-medium">Amount Threshold</label>
              <input value={(form.amountThreshold as string) || ''} onChange={(e) => setField('amountThreshold', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
            <div>
              <label className="text-xs font-medium">Currency</label>
              <input value={(form.currency as string) || ''} onChange={(e) => setField('currency', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
          </div>
          <div>
            <label className="text-xs font-medium">Expires After (minutes)</label>
            <input type="number" value={(form.expiresAfterMinutes as number) || ''} onChange={(e) => setField('expiresAfterMinutes', e.target.value ? parseInt(e.target.value) : undefined)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
          </div>
        </CardContent>
      </Card>

      <div className="flex gap-2">
        <Button onClick={handleSubmit} disabled={updateMutation.isPending}>{updateMutation.isPending ? 'Saving...' : 'Save Changes'}</Button>
        <Button variant="outline" onClick={() => router.back()}>Cancel</Button>
      </div>
    </div>
  );
}
