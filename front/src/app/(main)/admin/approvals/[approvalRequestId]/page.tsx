'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { PermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { toast } from 'sonner';
import { getApprovalRequest, approveApprovalRequest, rejectApprovalRequest, executeApprovalRequest, cancelApprovalRequest } from '@/api/admin-approvals';
import { CheckCircle, X, Play, Ban, AlertTriangle } from 'lucide-react';

function safeParseJson(str: string): Record<string, unknown> {
  try { return JSON.parse(str); }
  catch { return { raw: str.slice(0, 1000) }; }
}

export default function ApprovalDetailPage() {
  const { approvalRequestId } = useParams<{ approvalRequestId: string }>();
  const queryClient = useQueryClient();
  const [action, setAction] = useState<string | null>(null);
  const [reason, setReason] = useState('');

  const { data: approval, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['approval', approvalRequestId],
    queryFn: () => getApprovalRequest(approvalRequestId),
  });

  const actionMutation = useMutation({
    mutationFn: async (act: string) => {
      if (act === 'approve') return approveApprovalRequest(approvalRequestId);
      if (act === 'reject') return rejectApprovalRequest(approvalRequestId, { reason: reason || 'Rejected' });
      if (act === 'execute') return executeApprovalRequest(approvalRequestId);
      if (act === 'cancel') return cancelApprovalRequest(approvalRequestId, { reason: reason || 'Cancelled' });
      throw new Error('Unknown action');
    },
    onSuccess: () => {
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['approval', approvalRequestId] });
      setAction(null); setReason('');
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (isLoading) return <DetailPageSkeleton cards={4} />;
  if (isError || !approval) return <ErrorState error={(error as Error)?.message || t('notFound.approval')} onRetry={() => refetch()} />;

  const isPending = approval.status === 'pending';
  const isApproved = approval.status === 'approved';
  const isExpired = approval.status === 'expired';

  return (
    <div className="space-y-6">
      <div>
        <Link href="/admin/approvals" className="text-sm text-muted-foreground hover:text-foreground">← Approvals</Link>
        <div className="mt-1 flex items-center gap-3"><h1 className="text-xl font-bold">Request {approval.id.slice(0, 12)}…</h1><StatusBadge status={approval.status} /></div>
      </div>

      {isExpired && (
        <Card className="border-yellow-300 bg-yellow-50 dark:bg-yellow-950/20">
          <CardContent className="p-4 flex items-center gap-3"><AlertTriangle className="h-4 w-4 text-yellow-600" /><p className="text-xs">This request has expired. Approve/reject actions are disabled.</p></CardContent>
        </Card>
      )}

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Operation</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm font-medium capitalize">{approval.operationType.replace(/_/g, ' ')}</p></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Status</CardTitle></CardHeader><CardContent className="p-3 pt-0"><StatusBadge status={approval.status} /></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Approvals</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm font-bold">{approval.approvalsCount} / {approval.requiredApprovals}</p></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Expires</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm">{approval.expiresAt ? new Date(approval.expiresAt).toLocaleString() : '—'}</p></CardContent></Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Requester</span><span className="font-mono text-xs">{approval.requestedByUserId}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Subject</span><span>{approval.subjectType}: {approval.subjectId?.slice(0, 16)}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span>{new Date(approval.createdAt).toLocaleString()}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Updated</span><span>{new Date(approval.updatedAt).toLocaleString()}</span></div>
          </CardContent>
        </Card>

        {approval.decisions && approval.decisions.length > 0 && (
          <Card>
            <CardHeader><CardTitle className="text-sm">Decisions</CardTitle></CardHeader>
            <CardContent>
              <div className="space-y-2">
                {approval.decisions.map((d) => (
                  <div key={d.id} className="flex items-center justify-between border-b pb-2 last:border-0">
                    <div>
                      <span className="font-mono text-xs">{d.userId.slice(0, 8)}</span>
                      {d.note && <p className="text-xs text-muted-foreground">{d.note}</p>}
                    </div>
                    <div className="flex items-center gap-2">
                      <StatusBadge status={d.decision} />
                      <span className="text-xs text-muted-foreground">{new Date(d.decidedAt).toLocaleString()}</span>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      {approval.riskEvaluationId && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Linked Risk Evaluation</CardTitle></CardHeader>
          <CardContent>
            <Link href={`/admin/security/risk/evaluations/${approval.riskEvaluationId}`} className="font-mono text-xs text-primary hover:underline">{approval.riskEvaluationId}</Link>
          </CardContent>
        </Card>
      )}

      {approval.executionResult && <SafePayloadViewer data={safeParseJson(approval.executionResult)} title="Execution Result" />}
      {approval.requestPayload && <SafePayloadViewer data={approval.requestPayload} title="Request Payload" />}

      {/* Actions */}
      <div className="flex gap-2 flex-wrap">
        {isPending && (
          <>
            <PermissionGuard permission="payment.admin_approval.approve">
              <Button variant="outline" size="sm" onClick={() => setAction('approve')} className="text-green-600"><CheckCircle className="mr-1 h-3.5 w-3.5" /> Approve</Button>
            </PermissionGuard>
            <PermissionGuard permission="payment.admin_approval.reject">
              <Button variant="outline" size="sm" onClick={() => setAction('reject')} className="text-red-600"><X className="mr-1 h-3.5 w-3.5" /> Reject</Button>
            </PermissionGuard>
          </>
        )}
        {isApproved && (
          <PermissionGuard permission="payment.admin_approval.execute">
            <Button variant="outline" size="sm" onClick={() => setAction('execute')} className="text-blue-600"><Play className="mr-1 h-3.5 w-3.5" /> Execute</Button>
          </PermissionGuard>
        )}
        {(isPending || isApproved) && (
          <PermissionGuard permission="payment.admin_approval.cancel">
            <Button variant="outline" size="sm" onClick={() => setAction('cancel')} className="text-red-600"><Ban className="mr-1 h-3.5 w-3.5" /> Cancel</Button>
          </PermissionGuard>
        )}
      </div>

      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={() => actionMutation.mutate(action)}
          confirmDisabled={(action === 'reject' || action === 'cancel') && !reason}
          title={`${action === 'approve' ? 'Approve' : action === 'reject' ? 'Reject' : action === 'execute' ? 'Execute' : 'Cancel'} Request`}
          description={action === 'execute' ? 'This will execute the approved operation. Financial state may change. Are you sure?' : action === 'approve' ? 'Approve this request?' : <div className="mt-3"><label className="text-xs font-medium">Reason *</label><input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>}
          confirmLabel={action === 'approve' ? 'Approve' : action === 'reject' ? 'Reject' : action === 'execute' ? 'Execute' : 'Cancel'}
          destructive={action !== 'approve'} />
      )}
    </div>
  );
}
