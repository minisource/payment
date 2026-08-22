'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable, DataTableColumn, Pagination } from '@/components/shared/data-table';
import { StatusBadge, RiskSeverityBadge } from '@/components/shared/enhanced-badge';
import { FilterBar, RefreshButton, SearchInput } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { listRiskEvaluations, type RiskEvaluation } from '@/api/risk';

const PAGE_SIZE = 20;

export default function RiskEvaluationsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.risk.view']}>
      <EvaluationsContent />
    </RoutePermissionGuard>
  );
}

function EvaluationsContent() {
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [levelFilter, setLevelFilter] = useState('');
  const [decisionFilter, setDecisionFilter] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['risk-evaluations', { skip, search, riskLevel: levelFilter, decision: decisionFilter }],
    queryFn: () => listRiskEvaluations({ skip, take: PAGE_SIZE, query: search || undefined, riskLevel: levelFilter || undefined, decision: decisionFilter || undefined }),
  });

  const columns: DataTableColumn<RiskEvaluation>[] = [
    { key: 'id', header: 'ID', render: (e) => <Link href={`/admin/security/risk/evaluations/${e.id}`} className="font-mono text-xs text-primary hover:underline">{e.id.slice(0, 12)}…</Link> },
    { key: 'operation', header: 'Operation', render: (e) => <span className="text-xs capitalize">{e.operationType.replace(/_/g, ' ')}</span> },
    { key: 'subject', header: 'Subject', render: (e) => <span className="text-xs">{e.subjectType}: {e.subjectId?.slice(0, 8)}</span> },
    { key: 'level', header: 'Risk', render: (e) => <RiskSeverityBadge severity={e.riskLevel} /> },
    { key: 'score', header: 'Score', render: (e) => <span className={`text-xs font-bold ${e.riskScore >= 70 ? 'text-red-600' : e.riskScore >= 40 ? 'text-yellow-600' : 'text-green-600'}`}>{e.riskScore}</span> },
    { key: 'decision', header: 'Decision', render: (e) => <StatusBadge status={e.decision} /> },
    { key: 'rules', header: 'Rules', render: (e) => <span className="text-xs">{e.triggeredRulesCount}</span> },
    { key: 'created', header: 'Date', render: (e) => <span className="text-xs">{new Date(e.createdAt).toLocaleDateString()}</span> },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <Link href="/admin/security" className="text-sm text-muted-foreground hover:text-foreground">← Security</Link>
          <h1 className="text-2xl font-bold">Risk Evaluations</h1>
          <p className="text-sm text-muted-foreground">Risk scoring, triggered rules, and automated decisions</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <select value={levelFilter} onChange={(e) => { setLevelFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Levels</option><option value="low">Low</option><option value="medium">Medium</option><option value="high">High</option><option value="critical">Critical</option>
        </select>
        <select value={decisionFilter} onChange={(e) => { setDecisionFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Decisions</option><option value="allow">Allow</option><option value="challenge">Challenge</option><option value="require_approval">Require Approval</option><option value="block">Block</option><option value="review">Review</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(e) => e.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>
    </div>
  );
}
