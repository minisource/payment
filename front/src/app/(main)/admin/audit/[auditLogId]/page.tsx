'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { PageHeader } from '@/components/shared/page-header';
import { DetailCard } from '@/components/shared/detail-card';
import { toast } from 'sonner';
import { getAuditLog, verifyAuditHashChain } from '@/api/audit';
import { Shield } from 'lucide-react';

const HASH_STATUS_BADGE: Record<string, string> = {
  valid: 'bg-green-100 text-green-700',
  invalid: 'bg-red-100 text-red-700',
  unverified: 'bg-yellow-100 text-yellow-700',
  not_applicable: 'bg-gray-100 text-gray-500',
};

export default function AuditLogDetailPage() {
  return (
    <RoutePermissionGuard permissions={['payment.audit.view_admin']}>
      <AuditLogDetailContent />
    </RoutePermissionGuard>
  );
}

function AuditLogDetailContent() {
  const { auditLogId } = useParams<{ auditLogId: string }>();

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['audit-log', auditLogId],
    queryFn: () => getAuditLog(auditLogId as string),
    enabled: !!auditLogId,
  });

  const handleVerifyHash = async () => {
    try {
      const res = await verifyAuditHashChain({ auditLogId: auditLogId as string });
      toast.success(res.valid ? 'Hash chain verified' : 'Hash chain verification failed: ' + res.message);
    } catch (err: any) { toast.error(err.message || 'Hash verification failed'); }
  };

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !data) return <ErrorState error={(error as Error)?.message} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <PageHeader
        title="Audit Log Detail"
        backHref="/admin/audit"
        actions={
          <PermissionGuard permission="payment.audit.verify_hash_admin">
            <Button size="sm" variant="outline" onClick={handleVerifyHash}><Shield className="h-4 w-4 mr-1" />Verify Hash Chain</Button>
          </PermissionGuard>
        }
      />

      {/* Summary */}
      <DetailCard title="Audit Summary">
        <div className="grid gap-3 sm:grid-cols-3">
          <div><span className="text-xs text-muted-foreground">Audit Log ID</span><p className="font-mono text-sm">{data.id}</p></div>
          <div><span className="text-xs text-muted-foreground">Action</span><p className="text-sm font-mono font-medium">{data.action}</p></div>
          <div><span className="text-xs text-muted-foreground">Hash Status</span><p><span className={`rounded px-2 py-0.5 text-xs font-medium ${HASH_STATUS_BADGE[data.hashStatus] || ''}`}>{data.hashStatus.replace(/_/g, ' ')}</span></p></div>
        </div>
      </DetailCard>

      {/* Actor */}
      <DetailCard title="Actor">
        <div className="grid gap-3 sm:grid-cols-2">
          <div><span className="text-xs text-muted-foreground">Actor Type</span><p className="text-sm">{data.actorType}</p></div>
          <div><span className="text-xs text-muted-foreground">Actor User ID</span><p className="font-mono text-sm">{data.actorUserId || '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">IP Address</span><p className="font-mono text-sm">{data.ipAddress || '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">User Agent</span><p className="text-xs text-muted-foreground max-w-[300px] truncate">{data.userAgent || '—'}</p></div>
        </div>
      </DetailCard>

      {/* Entity */}
      <DetailCard title="Entity">
        <div className="grid gap-3 sm:grid-cols-3">
          <div><span className="text-xs text-muted-foreground">Entity Type</span><p className="text-sm">{data.entityType}</p></div>
          <div><span className="text-xs text-muted-foreground">Entity ID</span><p className="font-mono text-sm">{data.entityId}</p></div>
          <div><span className="text-xs text-muted-foreground">Tenant</span><p className="font-mono text-sm">{data.tenantId?.slice(0, 12) || '—'}…</p></div>
        </div>
      </DetailCard>

      {/* Request Info */}
      <DetailCard title="Request Info">
        <div className="grid gap-3 sm:grid-cols-2">
          <div><span className="text-xs text-muted-foreground">Request ID</span><p className="font-mono text-sm">{data.requestId || '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Correlation ID</span><p className="font-mono text-sm">{data.correlationId || '—'}</p></div>
          <div><span className="text-xs text-muted-foreground">Timestamp</span><p className="text-sm">{new Date(data.createdAt).toLocaleString()}</p></div>
        </div>
      </DetailCard>

      {/* Hash Chain Info */}
      {(data.hashAlgorithm || data.entryHash) && (
        <DetailCard title="Hash Chain">
          <div className="grid gap-3 sm:grid-cols-2">
            <div><span className="text-xs text-muted-foreground">Algorithm</span><p className="font-mono text-sm">{data.hashAlgorithm || '—'}</p></div>
            <div><span className="text-xs text-muted-foreground">Version</span><p className="text-sm">{data.hashVersion ?? '—'}</p></div>
            <div className="sm:col-span-2"><span className="text-xs text-muted-foreground">Previous Hash</span><p className="font-mono text-xs break-all">{data.previousHash || '—'}</p></div>
            <div className="sm:col-span-2"><span className="text-xs text-muted-foreground">Entry Hash</span><p className="font-mono text-xs break-all">{data.entryHash || '—'}</p></div>
          </div>
        </DetailCard>
      )}

      {/* Before Snapshot */}
      {data.beforeSnapshot && Object.keys(data.beforeSnapshot).length > 0 && (
        <DetailCard title="Before Snapshot">
          <SafePayloadViewer data={data.beforeSnapshot} />
        </DetailCard>
      )}

      {/* After Snapshot */}
      {data.afterSnapshot && Object.keys(data.afterSnapshot).length > 0 && (
        <DetailCard title="After Snapshot">
          <SafePayloadViewer data={data.afterSnapshot} />
        </DetailCard>
      )}

      {/* Metadata */}
      {data.metadata && Object.keys(data.metadata).length > 0 && (
        <DetailCard title="Metadata">
          <SafePayloadViewer data={data.metadata} />
        </DetailCard>
      )}

      <p className="text-xs text-muted-foreground text-center">Audit logs are immutable. No edit or delete operations are available.</p>
    </div>
  );
}
