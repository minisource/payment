'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { toast } from 'sonner';
import { getApprovalPolicy } from '@/api/approval-policies';

export default function ApprovalPolicyDetailPage() {
  const { approvalPolicyId } = useParams<{ approvalPolicyId: string }>();

  const { data: policy, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['approval-policy', approvalPolicyId],
    queryFn: () => getApprovalPolicy(approvalPolicyId),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !policy) return <ErrorState error={(error as Error)?.message || 'Not found'} onRetry={() => refetch()} />;

  return (
    <RoutePermissionGuard permissions={['payment.admin_approval.policy.view']}>
      <div className="space-y-6">
        <div>
          <Link href="/admin/approval-policies" className="text-sm text-muted-foreground hover:text-foreground">← Policies</Link>
          <div className="mt-1 flex items-center gap-3"><h1 className="text-xl font-bold">{policy.name}</h1><StatusBadge status={policy.status} /></div>
        </div>

        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Operation</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm font-medium capitalize">{policy.operationType.replace(/_/g, ' ')}</p></CardContent></Card>
          <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Status</CardTitle></CardHeader><CardContent className="p-3 pt-0"><StatusBadge status={policy.status} /></CardContent></Card>
          <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Required Approvals</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-lg font-bold">{policy.requiredApprovals}</p></CardContent></Card>
          <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Different User</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm">{policy.requireDifferentUser ? 'Required' : 'Not required'}</p></CardContent></Card>
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          <Card>
            <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
            <CardContent className="space-y-2 text-sm">
              {policy.description && <div className="flex justify-between"><span className="text-muted-foreground">Description</span><span>{policy.description}</span></div>}
              <div className="flex justify-between"><span className="text-muted-foreground">Scope</span><span>{policy.tenantId ? 'Tenant' : policy.applicationCode ? 'Application' : 'Global'}</span></div>
              <div className="flex justify-between"><span className="text-muted-foreground">Amount Threshold</span><span>{policy.amountThreshold ? `${policy.amountThreshold} ${policy.currency}` : 'Any'}</span></div>
              <div className="flex justify-between"><span className="text-muted-foreground">Expires After</span><span>{policy.expiresAfterMinutes ? `${policy.expiresAfterMinutes} min` : 'No expiry'}</span></div>
              <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span>{new Date(policy.createdAt).toLocaleString()}</span></div>
              <div className="flex justify-between"><span className="text-muted-foreground">Updated</span><span>{new Date(policy.updatedAt).toLocaleString()}</span></div>
            </CardContent>
          </Card>
        </div>

        {policy.conditions && (
          <Card>
            <CardHeader><CardTitle className="text-sm">Conditions</CardTitle></CardHeader>
            <CardContent>
              <pre className="text-xs font-mono bg-accent rounded p-3 overflow-auto">{JSON.stringify(policy.conditions, null, 2)}</pre>
            </CardContent>
          </Card>
        )}

        <div className="flex gap-2">
          <Link href={`/admin/approval-policies/${approvalPolicyId}/edit`}><Button variant="outline" size="sm">Edit Policy</Button></Link>
        </div>
      </div>
    </RoutePermissionGuard>
  );
}
