'use client';

import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { Loading, ErrorState } from '@/components/shared/states';
import { getSystemHealth, getReadiness, getLiveness, getDependencyHealth, getDispatcherStatus, type DependencyHealth, type DispatcherStatus } from '@/api/system-health';
import { Activity, Heart, Server, Database, RefreshCw, CheckCircle, XCircle, AlertTriangle, HelpCircle } from 'lucide-react';

const HEALTH_ICONS: Record<string, React.ComponentType<{ className?: string }>> = {
  healthy: CheckCircle,
  degraded: AlertTriangle,
  unhealthy: XCircle,
  unknown: HelpCircle,
};

const HEALTH_COLORS: Record<string, string> = {
  healthy: 'text-green-600',
  degraded: 'text-yellow-600',
  unhealthy: 'text-red-600',
  unknown: 'text-muted-foreground',
};

const HEALTH_BG: Record<string, string> = {
  healthy: 'bg-green-50 border-green-200',
  degraded: 'bg-yellow-50 border-yellow-200',
  unhealthy: 'bg-red-50 border-red-200',
  unknown: 'bg-gray-50',
};

export default function SystemHealthPage() {
  return (
    <RoutePermissionGuard permissions={['payment.system_health.view_admin']}>
      <HealthContent />
    </RoutePermissionGuard>
  );
}

function HealthCard({ title, status, icon: Icon, lastChecked, children }: { title: string; status: string; icon: React.ComponentType<{ className?: string }>; lastChecked?: string; children?: React.ReactNode }) {
  const StatusIcon = HEALTH_ICONS[status] || HelpCircle;
  return (
    <Card className={HEALTH_BG[status] || ''}>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <div className="flex items-center gap-2">
          <Icon className="h-5 w-5" />
          <CardTitle className="text-sm">{title}</CardTitle>
        </div>
        <StatusIcon className={`h-5 w-5 ${HEALTH_COLORS[status] || 'text-muted-foreground'}`} />
      </CardHeader>
      <CardContent>
        <div className="flex items-center gap-2">
          <span className="capitalize text-sm font-medium">{status}</span>
          {lastChecked && <span className="text-xs text-muted-foreground">{new Date(lastChecked).toLocaleTimeString()}</span>}
        </div>
        {children}
      </CardContent>
    </Card>
  );
}

function DependencyRow({ dep }: { dep: DependencyHealth }) {
  const StatusIcon = HEALTH_ICONS[dep.status] || HelpCircle;
  return (
    <div className="flex items-center justify-between py-2 border-b last:border-b-0">
      <div>
        <p className="text-sm font-medium">{dep.name}</p>
        <p className="text-xs text-muted-foreground">{dep.type}</p>
      </div>
      <div className="flex items-center gap-3">
        {dep.responseTimeMs != null && <span className="text-xs text-muted-foreground">{dep.responseTimeMs}ms</span>}
        <StatusIcon className={`h-4 w-4 ${HEALTH_COLORS[dep.status] || 'text-muted-foreground'}`} />
      </div>
      {dep.error && <p className="col-span-2 text-xs text-red-600 mt-1">{dep.error}</p>}
    </div>
  );
}

function DispatcherRow({ d }: { d: DispatcherStatus }) {
  const StatusIcon = HEALTH_ICONS[d.status] || HelpCircle;
  return (
    <div className="flex items-center justify-between py-2 border-b last:border-b-0">
      <div>
        <p className="text-sm font-medium">{d.name}</p>
        <p className="text-xs text-muted-foreground">{d.type}</p>
      </div>
      <div className="flex items-center gap-3">
        <span className="text-xs">Pending: {d.pendingCount}</span>
        <span className="text-xs">Processing: {d.processingCount}</span>
        <StatusIcon className={`h-4 w-4 ${d.isRunning ? 'text-green-600' : 'text-red-600'}`} />
      </div>
    </div>
  );
}

function HealthContent() {
  const { data: systemHealth, isLoading: hl, isError: he, error: herr, refetch: rh } = useQuery({ queryKey: ['system-health'], queryFn: getSystemHealth });
  const { data: readiness, refetch: rr } = useQuery({ queryKey: ['readiness'], queryFn: getReadiness });
  const { data: liveness, refetch: rl } = useQuery({ queryKey: ['liveness'], queryFn: getLiveness });
  const { data: deps, refetch: rd } = useQuery({ queryKey: ['deps-health'], queryFn: getDependencyHealth });
  const { data: dispatchers, refetch: rdisp } = useQuery({ queryKey: ['dispatchers'], queryFn: getDispatcherStatus });

  const handleRefresh = () => { rh(); rr(); rl(); rd(); rdisp(); };

  if (hl) return <Loading />;

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">System Health</h1>
          <p className="text-sm text-muted-foreground">API status, database, Redis, dispatcher, and gateway health</p>
        </div>
        <Button variant="outline" size="sm" onClick={handleRefresh}><RefreshCw className="h-4 w-4 mr-1" />Refresh</Button>
      </div>

      {/* API Health */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <HealthCard title="Payment API" status={systemHealth?.status || 'unknown'} icon={Server} lastChecked={systemHealth?.checkedAt}>
          {systemHealth?.uptime && <p className="mt-2 text-xs text-muted-foreground">Uptime: {systemHealth.uptime}</p>}
          {systemHealth?.version && <p className="text-xs text-muted-foreground">Version: {systemHealth.version}</p>}
        </HealthCard>
        <HealthCard title="Liveness" status={liveness?.status || 'unknown'} icon={Heart} lastChecked={liveness ? new Date().toISOString() : undefined} />
        <HealthCard title="Readiness" status={readiness?.status || 'unknown'} icon={Database} lastChecked={readiness ? new Date().toISOString() : undefined}>
          {readiness?.checks && readiness.checks.length > 0 && (
            <div className="mt-2 space-y-1">
              {readiness.checks.map((c, i) => {
                const Icon = HEALTH_ICONS[c.status] || HelpCircle;
                return (
                  <div key={i} className="flex items-center gap-2 text-xs">
                    <Icon className={`h-3 w-3 ${HEALTH_COLORS[c.status]}`} />
                    <span>{c.name}</span>
                    {c.responseTimeMs != null && <span className="text-muted-foreground">{c.responseTimeMs}ms</span>}
                  </div>
                );
              })}
            </div>
          )}
        </HealthCard>
      </div>

      {/* Dependencies */}
      {deps?.items && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Dependencies</CardTitle></CardHeader>
          <CardContent className="p-0">
            <div className="px-4 divide-y">
              {deps.items.map((dep) => (
                <DependencyRow key={dep.name} dep={dep} />
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Dispatchers */}
      {dispatchers?.items && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Dispatchers / Workers</CardTitle></CardHeader>
          <CardContent className="p-0">
            <div className="px-4 divide-y">
              {dispatchers.items.map((d) => (
                <DispatcherRow key={d.name} d={d} />
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Gateway Health Summary - placeholder */}
      <Card>
        <CardHeader><CardTitle className="text-sm">Gateway Provider Health</CardTitle></CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Gateway provider health data is available via the Gateway Providers page.</p>
          <a href="/admin/gateways/providers" className="text-xs text-primary hover:underline mt-1 inline-block">View Gateway Providers →</a>
        </CardContent>
      </Card>

      {he && (
        <Card className="border-red-200 bg-red-50"><CardContent className="p-4"><p className="text-sm text-red-700">System health API unavailable: {(herr as Error)?.message}</p></CardContent></Card>
      )}
    </div>
  );
}
