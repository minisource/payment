'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { toast } from 'sonner';
import { createApprovalPolicy, type ApprovalPolicyCreateRequest } from '@/api/approval-policies';

export default function NewApprovalPolicyPage() {
  return (
    <RoutePermissionGuard permissions={['payment.admin_approval.policy.create']}>
      <NewPolicyContent />
    </RoutePermissionGuard>
  );
}

function NewPolicyContent() {
  const router = useRouter();
  const [form, setForm] = useState<ApprovalPolicyCreateRequest>({
    name: '',
    operationType: '',
    requiredApprovals: 2,
    requireDifferentUser: true,
  });

  const createMutation = useMutation({
    mutationFn: () => createApprovalPolicy(form),
    onSuccess: (data) => { toast.success('Policy created'); router.push(`/admin/approval-policies/${data.id}`); },
    onError: (err: Error) => toast.error(err.message),
  });

  const handleSubmit = () => {
    if (!form.name || !form.operationType) { toast.error('Name and operation type are required'); return; }
    if (form.requiredApprovals < 1) { toast.error('At least 1 approval required'); return; }
    createMutation.mutate();
  };

  const setField = (k: keyof ApprovalPolicyCreateRequest, v: any) => setForm((p) => ({ ...p, [k]: v }));

  return (
    <div className="space-y-6 max-w-2xl">
      <div>
        <Link href="/admin/approval-policies" className="text-sm text-muted-foreground hover:text-foreground">← Policies</Link>
        <h1 className="text-2xl font-bold">New Approval Policy</h1>
        <p className="text-sm text-muted-foreground">Create a maker-checker approval rule</p>
      </div>

      <Card>
        <CardHeader><CardTitle className="text-sm">Policy Details</CardTitle></CardHeader>
        <CardContent className="space-y-4">
          <div>
            <label className="text-xs font-medium">Name *</label>
            <input value={form.name} onChange={(e) => setField('name', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="e.g. High-value withdrawal approval" />
          </div>
          <div>
            <label className="text-xs font-medium">Description</label>
            <input value={form.description || ''} onChange={(e) => setField('description', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="Optional description" />
          </div>
          <div>
            <label className="text-xs font-medium">Operation Type *</label>
            <select value={form.operationType} onChange={(e) => setField('operationType', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm">
              <option value="">Select operation...</option>
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
              <label className="text-xs font-medium">Required Approvals *</label>
              <input type="number" min={1} value={form.requiredApprovals} onChange={(e) => setField('requiredApprovals', parseInt(e.target.value) || 1)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
            <div>
              <label className="text-xs font-medium">Different User Required</label>
              <select value={form.requireDifferentUser ? 'true' : 'false'} onChange={(e) => setField('requireDifferentUser', e.target.value === 'true')} className="mt-1 w-full rounded-md border px-3 py-2 text-sm">
                <option value="true">Yes</option><option value="false">No</option>
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="text-xs font-medium">Amount Threshold</label>
              <input value={form.amountThreshold || ''} onChange={(e) => setField('amountThreshold', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="e.g. 100000" />
            </div>
            <div>
              <label className="text-xs font-medium">Currency</label>
              <input value={form.currency || ''} onChange={(e) => setField('currency', e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="e.g. IRT" />
            </div>
          </div>
          <div>
            <label className="text-xs font-medium">Expires After (minutes)</label>
            <input type="number" value={form.expiresAfterMinutes || ''} onChange={(e) => setField('expiresAfterMinutes', e.target.value ? parseInt(e.target.value) : undefined)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="Leave empty for no expiry" />
          </div>
        </CardContent>
      </Card>

      <div className="flex gap-2">
        <Button onClick={handleSubmit} disabled={createMutation.isPending}>{createMutation.isPending ? 'Creating...' : 'Create Policy'}</Button>
        <Button variant="outline" onClick={() => router.back()}>Cancel</Button>
      </div>
    </div>
  );
}
