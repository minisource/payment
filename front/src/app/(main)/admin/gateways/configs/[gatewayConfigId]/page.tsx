'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { ConfirmDialog } from '@/components/shared/components';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { PermissionGuard } from '@/components/shared/permission-guards';
import { EnvironmentBadge, ScopeBadge, ProviderBadge, GatewayHealthBadge } from '@/components/shared/gateway-badges';
import { toast } from 'sonner';
import {
  getGatewayConfig, enableGatewayConfig, disableGatewayConfig, deleteGatewayConfig, testGatewayConfig,
  type GatewayConfigTestResult,
} from '@/api/gateways';
import { Shield, AlertTriangle, CheckCircle, Power, PowerOff, Trash2, Play } from 'lucide-react';

export default function GatewayConfigDetailPage() {
  const { gatewayConfigId } = useParams<{ gatewayConfigId: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();
  const [confirmAction, setConfirmAction] = useState<'disable' | 'delete' | 'test' | null>(null);
  const [testResult, setTestResult] = useState<GatewayConfigTestResult | null>(null);

  const { data: config, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['gateway-config', gatewayConfigId],
    queryFn: () => getGatewayConfig(gatewayConfigId),
  });

  const toggleMutation = useMutation({
    mutationFn: () => config?.status === 'active' ? disableGatewayConfig(gatewayConfigId) : enableGatewayConfig(gatewayConfigId),
    onSuccess: () => { toast.success(config?.status === 'active' ? 'Config disabled' : 'Config enabled'); queryClient.invalidateQueries({ queryKey: ['gateway-config', gatewayConfigId] }); },
    onError: (err: Error) => toast.error(err.message),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteGatewayConfig(gatewayConfigId),
    onSuccess: () => { toast.success('Config deleted'); router.push('/admin/gateways/configs'); },
    onError: (err: Error) => toast.error(err.message),
  });

  const testMutation = useMutation({
    mutationFn: () => testGatewayConfig(gatewayConfigId),
    onSuccess: (res) => { setTestResult(res); setConfirmAction(null); },
    onError: (err: Error) => toast.error(err.message),
  });

  if (isLoading) return <DetailPageSkeleton cards={4} />;
  if (isError || !config) return <ErrorState error={(error as Error)?.message || 'Config not found'} onRetry={() => refetch()} />;

  const isProduction = config.environment === 'production';

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <Link href="/admin/gateways/configs" className="text-sm text-muted-foreground hover:text-foreground">← Gateway Configs</Link>
          <div className="mt-1 flex items-center gap-3">
            <h1 className="text-xl font-bold">{config.name}</h1>
            <StatusBadge status={config.status} />
            <EnvironmentBadge environment={config.environment} />
          </div>
          <p className="text-xs font-mono text-muted-foreground">{config.id}</p>
        </div>
        <div className="flex items-center gap-2">
          <PermissionGuard permission="payment.gateway.config.update">
            <Button variant="outline" size="sm" onClick={() => router.push(`/admin/gateways/configs/${gatewayConfigId}/edit`)}>Edit</Button>
            {config.status === 'active' ? (
              <Button variant="outline" size="sm" onClick={() => setConfirmAction('disable')} className="text-amber-600"><PowerOff className="mr-1 h-3.5 w-3.5" /> Disable</Button>
            ) : (
              <Button variant="outline" size="sm" onClick={() => toggleMutation.mutate()}><Power className="mr-1 h-3.5 w-3.5" /> Enable</Button>
            )}
            <Button variant="outline" size="sm" onClick={() => setConfirmAction('test')}><Play className="mr-1 h-3.5 w-3.5" /> Test</Button>
            <Button variant="outline" size="sm" onClick={() => setConfirmAction('delete')} className="text-red-600"><Trash2 className="mr-1 h-3.5 w-3.5" /> Delete</Button>
          </PermissionGuard>
        </div>
      </div>

      {isProduction && (
        <Card className="border-amber-500 bg-amber-50 dark:bg-amber-950/20">
          <CardContent className="flex items-center gap-3 p-3">
            <AlertTriangle className="h-5 w-5 text-amber-600" />
            <div>
              <p className="text-sm font-medium text-amber-800 dark:text-amber-400">Production Configuration</p>
              <p className="text-xs text-amber-700 dark:text-amber-500">Changes to this configuration may affect live payments. Proceed with caution.</p>
            </div>
          </CardContent>
        </Card>
      )}

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Configuration</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Name</span><span className="font-medium">{config.name}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Description</span><span className="text-xs">{config.description || '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Provider</span><ProviderBadge provider={config.providerCode} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Scope</span><ScopeBadge scope={config.scope} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Environment</span><EnvironmentBadge environment={config.environment} /></div>
            {config.tenantId && <div className="flex justify-between"><span className="text-muted-foreground">Tenant</span><span className="font-mono text-xs">{config.tenantId}</span></div>}
            {config.applicationCode && <div className="flex justify-between"><span className="text-muted-foreground">Application</span><span className="font-mono text-xs">{config.applicationCode}</span></div>}
            <div className="flex justify-between"><span className="text-muted-foreground">Default</span><span>{config.isDefault ? 'Yes' : 'No'}</span></div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Routing & Limits</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Priority</span><span className="font-mono text-xs">{config.priority}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Weight</span><span className="font-mono text-xs">{config.weight}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Min Amount</span><span>{config.minAmount != null ? <MoneyAmount amount={config.minAmount} currency={config.defaultCurrency} /> : '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Max Amount</span><span>{config.maxAmount != null ? <MoneyAmount amount={config.maxAmount} currency={config.defaultCurrency} /> : '—'}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Health</span><GatewayHealthBadge health={config.healthStatus} /></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span className="text-xs">{new Date(config.createdAt).toLocaleString()}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Updated</span><span className="text-xs">{new Date(config.updatedAt).toLocaleString()}</span></div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><CardTitle className="text-sm">Currencies</CardTitle></CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-1">
            {config.supportedCurrencies?.map((c) => <span key={c} className="rounded bg-muted px-2 py-1 text-xs font-mono">{c}</span>)}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle className="text-sm flex items-center gap-2"><Shield className="h-4 w-4" /> Secret Fields Status</CardTitle></CardHeader>
        <CardContent>
          {config.secretFieldsStatus?.length ? (
            <div className="space-y-2">
              {config.secretFieldsStatus.map((s) => (
                <div key={s.key} className="flex items-center justify-between rounded-md border p-2">
                  <div className="flex items-center gap-2">
                    {s.configured ? <CheckCircle className="h-4 w-4 text-green-600" /> : <AlertTriangle className="h-4 w-4 text-amber-600" />}
                    <span className="text-xs font-mono font-medium">{s.key}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className={s.configured ? 'text-xs text-green-600 font-medium' : 'text-xs text-amber-600'}>
                      {s.configured ? 'Configured' : 'Not configured'}
                    </span>
                    {s.updatedAt && <span className="text-xs text-muted-foreground">Updated: {new Date(s.updatedAt).toLocaleDateString()}</span>}
                  </div>
                </div>
              ))}
            </div>
          ) : <p className="text-sm text-muted-foreground">No secret fields</p>}
        </CardContent>
      </Card>

      {config.configValues && Object.keys(config.configValues).length > 0 && (
        <SafePayloadViewer data={config.configValues} title="Config Values (Non-Secret)" />
      )}

      {testResult && (
        <Card className={testResult.success ? 'border-green-500' : 'border-red-500'}>
          <CardContent className="flex items-center gap-3 p-4">
            {testResult.success ? <CheckCircle className="h-5 w-5 text-green-600" /> : <AlertTriangle className="h-5 w-5 text-red-600" />}
            <div>
              <p className="text-sm font-medium">{testResult.message}</p>
              <p className="text-xs text-muted-foreground">Status: {testResult.status} · Checked: {new Date(testResult.checkedAt).toLocaleString()}</p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Confirm dialogs */}
      <ConfirmDialog open={confirmAction === 'disable'} onClose={() => setConfirmAction(null)}
        onConfirm={() => { toggleMutation.mutate(); setConfirmAction(null); }}
        title="Disable Gateway Config" description={isProduction ? 'This is a PRODUCTION config. Disabling it may affect live payments.' : 'Are you sure you want to disable this config?'}
        confirmLabel="Disable" destructive />

      <ConfirmDialog open={confirmAction === 'delete'} onClose={() => setConfirmAction(null)}
        onConfirm={() => { deleteMutation.mutate(); setConfirmAction(null); }}
        title="Delete Gateway Config" description={isProduction ? 'This is a PRODUCTION config. Deleting it may cause payment failures.' : 'Are you sure you want to delete this config? This action cannot be undone.'}
        confirmLabel="Delete" destructive />

      <ConfirmDialog open={confirmAction === 'test'} onClose={() => setConfirmAction(null)}
        onConfirm={() => testMutation.mutate()}
        title="Test Gateway Config" description={isProduction ? 'Testing a production config may contact the real gateway provider. Proceed?' : 'This will test the gateway configuration connectivity.'}
        confirmLabel={testMutation.isPending ? 'Testing…' : 'Test'} destructive={isProduction} />
    </div>
  );
}
