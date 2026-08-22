'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { StatusBadge, RiskSeverityBadge } from '@/components/shared/enhanced-badge';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { getRiskCase } from '@/api/risk';
import { AlertTriangle } from 'lucide-react';

export default function RiskCaseDetailPage() {
  const { caseId } = useParams<{ caseId: string }>();

  const { data: riskCase, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['risk-case', caseId],
    queryFn: () => getRiskCase(caseId),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !riskCase) return <ErrorState error={(error as Error)?.message || 'Not found'} onRetry={() => refetch()} />;

  const isCritical = riskCase.severity === 'critical';

  return (
    <div className="space-y-6">
      <div>
        <Link href="/admin/security/risk/cases" className="text-sm text-muted-foreground hover:text-foreground">← Cases</Link>
        <div className="mt-1 flex items-center gap-3"><h1 className="text-xl font-bold">{riskCase.title}</h1><StatusBadge status={riskCase.status} /></div>
      </div>

      {isCritical && (
        <Card className="border-red-300 bg-red-50 dark:bg-red-950/20">
          <CardContent className="p-4 flex items-center gap-3">
            <AlertTriangle className="h-5 w-5 text-red-600" />
            <div><p className="text-sm font-semibold text-red-700 dark:text-red-400">Critical Risk Case</p><p className="text-xs text-red-600 dark:text-red-400">This case requires immediate attention. Strong confirmation is required for closure actions.</p></div>
          </CardContent>
        </Card>
      )}

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Type</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm font-medium capitalize">{riskCase.caseType.replace(/_/g, ' ')}</p></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Severity</CardTitle></CardHeader><CardContent className="p-3 pt-0"><RiskSeverityBadge severity={riskCase.severity} /></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Status</CardTitle></CardHeader><CardContent className="p-3 pt-0"><StatusBadge status={riskCase.status} /></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Assigned</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-xs font-mono">{riskCase.assignedToUserId ? riskCase.assignedToUserId.slice(0, 12) : 'Unassigned'}</p></CardContent></Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Subject</span><span>{riskCase.subjectType}: {riskCase.subjectId?.slice(0, 16)}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span>{new Date(riskCase.createdAt).toLocaleString()}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Updated</span><span>{new Date(riskCase.updatedAt).toLocaleString()}</span></div>
          </CardContent>
        </Card>
      </div>

      {riskCase.description && <Card><CardContent className="p-4"><p className="text-sm font-medium">Description</p><p className="mt-1 text-xs text-muted-foreground">{riskCase.description}</p></CardContent></Card>}

      {riskCase.timeline && riskCase.timeline.length > 0 && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Timeline</CardTitle></CardHeader>
          <CardContent>
            <div className="space-y-3">
              {riskCase.timeline.map((t, i) => (
                <div key={i} className="flex gap-3 border-l-2 border-muted pl-3">
                  <div>
                    <p className="text-xs font-medium capitalize">{t.status.replace(/_/g, ' ')}</p>
                    <p className="text-xs text-muted-foreground">{new Date(t.timestamp).toLocaleString()}</p>
                    {t.userId && <p className="text-xs text-muted-foreground font-mono">by {t.userId.slice(0, 8)}</p>}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {riskCase.relatedEvaluationIds && riskCase.relatedEvaluationIds.length > 0 && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Related Evaluations</CardTitle></CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-2">
              {riskCase.relatedEvaluationIds.map((eid) => (
                <Link key={eid} href={`/admin/security/risk/evaluations/${eid}`} className="font-mono text-xs text-primary hover:underline">{eid.slice(0, 12)}…</Link>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {riskCase.resolutionNote && <Card><CardContent className="p-4"><p className="text-sm font-medium">Resolution</p><p className="mt-1 text-xs text-muted-foreground">{riskCase.resolutionNote}</p></CardContent></Card>}
      {riskCase.metadata && <SafePayloadViewer data={riskCase.metadata} title="Metadata" />}
    </div>
  );
}
