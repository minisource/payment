'use client';

import { useParams, useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { DataTable, DataTableColumn } from '@/components/shared/data-table';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { PermissionGuard } from '@/components/shared/permission-guards';
import { StrategyBadge, ScopeBadge, ProviderBadge } from '@/components/shared/gateway-badges';
import { getRoutingPolicy, type RoutingRule } from '@/api/gateways';
import { GitBranch, ArrowLeftRight } from 'lucide-react';

export default function RoutingPolicyDetailPage() {
  const { routingPolicyId } = useParams<{ routingPolicyId: string }>();
  const router = useRouter();

  const { data: policy, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['routing-policy', routingPolicyId],
    queryFn: () => getRoutingPolicy(routingPolicyId),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !policy) return <ErrorState error={(error as Error)?.message || 'Policy not found'} onRetry={() => refetch()} />;

  const ruleColumns: DataTableColumn<RoutingRule>[] = [
    { key: 'priority', header: 'Priority', render: (r) => <span className="font-mono text-xs">{r.priority}</span> },
    { key: 'provider', header: 'Provider', render: (r) => <ProviderBadge provider={r.providerCode} /> },
    { key: 'configId', header: 'Config', render: (r) => <Link href={`/admin/gateways/configs/${r.gatewayConfigId}`} className="font-mono text-xs text-primary hover:underline">{r.gatewayConfigId.slice(0, 12)}…</Link> },
    { key: 'weight', header: 'Weight', render: (r) => <span className="font-mono text-xs">{r.weight}</span> },
    { key: 'currency', header: 'Currency', render: (r) => <span className="text-xs font-mono">{r.currency || '*'}</span> },
    { key: 'amountRange', header: 'Amount Range', render: (r) => <span className="text-xs text-muted-foreground">{r.minAmount != null || r.maxAmount != null ? `${r.minAmount || '0'}–${r.maxAmount || '∞'}` : 'Any'}</span> },
    { key: 'status', header: 'Status', render: (r) => <StatusBadge status={r.status} /> },
  ];

  const strategyDesc: Record<string, string> = {
    priority: 'Route to the eligible config with the lowest priority number (highest priority).',
    random: 'Select a random eligible config uniformly.',
    weighted_random: 'Select a random eligible config based on configured weights.',
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <Link href="/admin/gateways/routing-policies" className="text-sm text-muted-foreground hover:text-foreground">← Routing Policies</Link>
          <div className="mt-1 flex items-center gap-3">
            <h1 className="text-xl font-bold">{policy.name}</h1>
            <StatusBadge status={policy.status} />
            <StrategyBadge strategy={policy.strategy} />
            <ScopeBadge scope={policy.scope} />
          </div>
          <p className="text-xs font-mono text-muted-foreground">{policy.id}</p>
        </div>
        <PermissionGuard permission="payment.gateway.routing.manage">
          <Button variant="outline" size="sm" onClick={() => router.push(`/admin/gateways/routing-policies/${routingPolicyId}/edit`)}>Edit Policy</Button>
        </PermissionGuard>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm flex items-center gap-2"><GitBranch className="h-4 w-4" /> Strategy</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Strategy</span><StrategyBadge strategy={policy.strategy} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Scope</span><ScopeBadge scope={policy.scope} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Fallback</span><span>{policy.fallbackEnabled ? 'Enabled' : 'Disabled'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Rules</span><span className="font-mono">{policy.rules?.length || 0}</span></div>
            {policy.description && <div className="flex justify-between"><span className="text-muted-foreground">Description</span><span className="text-xs">{policy.description}</span></div>}
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span className="text-xs">{new Date(policy.createdAt).toLocaleString()}</span></div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm flex items-center gap-2"><ArrowLeftRight className="h-4 w-4" /> How It Works</CardTitle></CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">{strategyDesc[policy.strategy] || 'Unknown strategy'}</p>
            {policy.fallbackEnabled && <p className="mt-2 text-xs text-muted-foreground">If no rules match, the fallback mechanism will attempt to route through any available config.</p>}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><CardTitle className="text-sm">Routing Rules</CardTitle></CardHeader>
        <CardContent className="p-0">
          <DataTable columns={ruleColumns} data={policy.rules || []} keyExtractor={(r) => r.gatewayConfigId}
            isEmpty={!policy.rules?.length} emptyMessage="No rules configured" />
        </CardContent>
      </Card>
    </div>
  );
}
