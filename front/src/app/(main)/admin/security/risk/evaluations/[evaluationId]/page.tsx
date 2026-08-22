'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { StatusBadge, RiskSeverityBadge } from '@/components/shared/enhanced-badge';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { getRiskEvaluation } from '@/api/risk';

export default function RiskEvaluationDetailPage() {
  const { evaluationId } = useParams<{ evaluationId: string }>();

  const { data: eval_, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['risk-evaluation', evaluationId],
    queryFn: () => getRiskEvaluation(evaluationId),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !eval_) return <ErrorState error={(error as Error)?.message || 'Not found'} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <div>
        <Link href="/admin/security/risk/evaluations" className="text-sm text-muted-foreground hover:text-foreground">← Evaluations</Link>
        <div className="mt-1 flex items-center gap-3"><h1 className="text-xl font-bold">Evaluation {eval_.id.slice(0, 12)}…</h1><RiskSeverityBadge severity={eval_.riskLevel} /></div>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Operation</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className="text-sm font-medium capitalize">{eval_.operationType.replace(/_/g, ' ')}</p></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Risk Level</CardTitle></CardHeader><CardContent className="p-3 pt-0"><RiskSeverityBadge severity={eval_.riskLevel} /></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Risk Score</CardTitle></CardHeader><CardContent className="p-3 pt-0"><p className={`text-lg font-bold ${eval_.riskScore >= 70 ? 'text-red-600' : eval_.riskScore >= 40 ? 'text-yellow-600' : 'text-green-600'}`}>{eval_.riskScore}</p></CardContent></Card>
        <Card><CardHeader className="p-3"><CardTitle className="text-xs text-muted-foreground">Decision</CardTitle></CardHeader><CardContent className="p-3 pt-0"><StatusBadge status={eval_.decision} /></CardContent></Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Subject</span><span>{eval_.subjectType}: {eval_.subjectId}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Rules Triggered</span><span>{eval_.triggeredRulesCount}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span>{new Date(eval_.createdAt).toLocaleString()}</span></div>
          </CardContent>
        </Card>
      </div>

      {eval_.outputReason && <Card><CardContent className="p-4"><p className="text-sm font-medium">Decision Reason</p><p className="mt-1 text-xs text-muted-foreground">{eval_.outputReason}</p></CardContent></Card>}
      {eval_.triggeredRules && eval_.triggeredRules.length > 0 && (
        <Card><CardHeader><CardTitle className="text-sm">Triggered Rules</CardTitle></CardHeader><CardContent><ul className="list-disc pl-4 space-y-1">{eval_.triggeredRules.map((r, i) => <li key={i} className="text-xs font-mono">{r}</li>)}</ul></CardContent></Card>
      )}
      {eval_.inputContext && <SafePayloadViewer data={eval_.inputContext} title="Input Context" />}
    </div>
  );
}
