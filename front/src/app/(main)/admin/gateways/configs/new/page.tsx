'use client';

import { useState, useMemo } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { SecretInput } from '@/components/shared/secret-input';
import { Loading, ErrorState } from '@/components/shared/states';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { toast } from 'sonner';
import { createGatewayConfig, listGatewayProviders, type GatewayConfigCreateRequest, type GatewayProviderDetail } from '@/api/gateways';
import { ArrowLeft, Save, AlertTriangle, CheckCircle, Info, Banknote, Shield, Globe, Server } from 'lucide-react';

export default function NewGatewayConfigPage() {
  return (
    <RoutePermissionGuard permissions={['payment.gateway.config.create']}>
      <NewConfigForm />
    </RoutePermissionGuard>
  );
}

const PROVIDER_ICONS: Record<string, React.ReactNode> = {
  zarinpal: <Banknote className="h-4 w-4 text-blue-500" />,
  idpay: <Banknote className="h-4 w-4 text-green-500" />,
  payir: <Banknote className="h-4 w-4 text-purple-500" />,
  zibal: <Banknote className="h-4 w-4 text-orange-500" />,
  snapp: <Banknote className="h-4 w-4 text-pink-500" />,
  parbad_virtual: <Server className="h-4 w-4 text-emerald-500" />,
};

/** Per-provider credential field definitions with their expected secret keys */
interface ProviderCredential {
  key: string;
  label: string;
}

const PROVIDER_CREDENTIALS: Record<string, ProviderCredential[]> = {
  zarinpal: [{ key: 'merchant_id', label: 'Merchant ID' }],
  idpay: [{ key: 'api_key', label: 'API Key' }],
  payir: [{ key: 'api_key', label: 'API Key' }],
  zibal: [{ key: 'merchant_id', label: 'Merchant ID' }],
  snapp: [
    { key: 'api_key', label: 'API Key' },
    { key: 'terminal_id', label: 'Terminal ID' },
  ],
  parbad_virtual: [{ key: 'gateway_path', label: 'Gateway Path' }],
};

function NewConfigForm() {
  const router = useRouter();

  // Fetch providers for dropdown
  const { data: providersData, isLoading: providersLoading, isError: providersError, error: providersErr, refetch: refetchProviders } = useQuery({
    queryKey: ['gateway-providers', 'active'],
    queryFn: () => listGatewayProviders({ status: 'active' }),
    staleTime: 60_000,
    retry: 1,
  });
  const providers = providersData?.items || [];

  const [form, setForm] = useState({
    name: '', description: '', providerCode: '',
    scope: 'tenant' as 'global' | 'tenant' | 'application',
    tenantId: '', applicationCode: '',
    environment: 'sandbox' as 'sandbox' | 'production',
    priority: '100', weight: '1',
    minAmount: '', maxAmount: '',
    supportedCurrencies: 'IRT',
    defaultCurrency: 'IRT',
    configValues: {} as Record<string, string>,
    secretValues: {} as Record<string, string>,
  });

  const selectedProvider = useMemo(
    () => providers.find(p => p.code === form.providerCode),
    [providers, form.providerCode]
  );

  // Credential fields: use known definitions first, fall back to configSchema
  const credentialFields = useMemo((): ProviderCredential[] => {
    if (!form.providerCode) return [];
    const known = PROVIDER_CREDENTIALS[form.providerCode];
    if (known) return known;
    // Fallback: try to read from provider's configSchema (if populated)
    const schema = selectedProvider?.configSchema;
    if (schema?.length) {
      return schema
        .filter(f => f.sensitive)
        .map(f => ({ key: f.key, label: f.label }));
    }
    return [];
  }, [form.providerCode, selectedProvider]);

  const handleSelectProvider = (code: string) => {
    const provider = providers.find(p => p.code === code);
    setForm(prev => ({
      ...prev,
      providerCode: code,
      name: `${provider?.displayName || code} (Sandbox)`,
      supportedCurrencies: provider?.supportedCurrencies?.join(', ') || prev.supportedCurrencies,
      defaultCurrency: provider?.supportedCurrencies?.[0] || prev.defaultCurrency,
      secretValues: {}, // Reset secrets on provider change
    }));
  };

  const mutation = useMutation({
    mutationFn: (req: GatewayConfigCreateRequest) => createGatewayConfig(req),
    onSuccess: (data) => {
      toast.success('Gateway config created');
      router.push(`/admin/gateways/configs/${data.id}`);
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name || !form.providerCode) {
      toast.error('Name and provider are required');
      return;
    }
    const req: GatewayConfigCreateRequest = {
      name: form.name, providerCode: form.providerCode,
      description: form.description || undefined,
      scope: form.scope,
      tenantId: form.scope !== 'global' ? form.tenantId || undefined : undefined,
      applicationCode: form.scope === 'application' ? form.applicationCode || undefined : undefined,
      environment: form.environment,
      supportedCurrencies: form.supportedCurrencies.split(',').map(c => c.trim()).filter(Boolean),
      defaultCurrency: form.defaultCurrency || undefined,
      priority: form.priority ? parseInt(form.priority) : undefined,
      weight: form.weight ? parseInt(form.weight) : undefined,
      minAmount: form.minAmount || undefined, maxAmount: form.maxAmount || undefined,
      configValues: Object.keys(form.configValues).length ? form.configValues : undefined,
      secretValues: Object.keys(form.secretValues).length ? form.secretValues : undefined,
    };
    mutation.mutate(req);
  };

  if (providersLoading) return <Loading />;
  if (providersError) return (
    <ErrorState
      error={(providersErr as Error)?.message || 'Failed to load providers'}
      onRetry={() => refetchProviders()}
    />
  );

  const activeProviders = providers.filter(p => p.enabled);

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="flex items-center gap-3">
        <button onClick={() => router.back()} className="text-muted-foreground hover:text-foreground"><ArrowLeft className="h-5 w-5" /></button>
        <h1 className="text-2xl font-bold">New Gateway Config</h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* ── Provider Selector ─────────────────────────── */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm">Payment Provider</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
              {activeProviders.map((p) => (
                <button
                  key={p.code}
                  type="button"
                  onClick={() => handleSelectProvider(p.code)}
                  className={`flex items-center gap-2 rounded-lg border p-3 text-left text-sm transition-all
                    ${form.providerCode === p.code
                      ? 'border-primary bg-primary/10 ring-1 ring-primary shadow-sm'
                      : 'hover:border-primary/40 hover:bg-accent/50'}`}
                >
                  <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md bg-muted">
                    {PROVIDER_ICONS[p.code] || <Server className="h-4 w-4" />}
                  </div>
                  <div className="min-w-0">
                    <div className="truncate font-medium">{p.displayName}</div>
                    <div className="truncate text-[10px] text-muted-foreground font-mono">{p.code}</div>
                  </div>
                  {form.providerCode === p.code && (
                    <CheckCircle className="ml-auto h-4 w-4 shrink-0 text-primary" />
                  )}
                </button>
              ))}
              {activeProviders.length === 0 && (
                <div className="col-span-full py-8 text-center text-sm text-muted-foreground">
                  <Info className="mx-auto mb-2 h-8 w-8 opacity-40" />
                  No active providers available. Seed data or create a provider first.
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* ── Provider Info (when selected) ────────────── */}
        {selectedProvider && (
          <Card className="border-primary/30 bg-primary/5">
            <CardContent className="flex flex-wrap items-center gap-4 p-3 text-xs">
              <span className="flex items-center gap-1">
                <Banknote className="h-3.5 w-3.5 text-muted-foreground" />
                <span className="text-muted-foreground">Currencies:</span>
                <span className="font-mono font-medium">{selectedProvider.supportedCurrencies.join(', ')}</span>
              </span>
              <span className="flex items-center gap-1">
                <Globe className="h-3.5 w-3.5 text-muted-foreground" />
                <span className="text-muted-foreground">Operations:</span>
                <span className="font-medium">{selectedProvider.supportedOperations.join(', ')}</span>
              </span>
              <span className="flex items-center gap-1 ml-auto">
                <Shield className="h-3.5 w-3.5 text-muted-foreground" />
                <span className="text-muted-foreground">Adapter:</span>
                <span className="font-mono font-medium">{selectedProvider.adapterType}</span>
              </span>
            </CardContent>
          </Card>
        )}

        {/* ── Basic Info ───────────────────────────────── */}
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm">Basic Info</CardTitle></CardHeader>
          <CardContent className="space-y-3">
            <div>
              <label className="text-xs font-medium">Name *</label>
              <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })}
                placeholder="e.g. Zarinpal Production" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" required />
            </div>
            <div>
              <label className="text-xs font-medium">Description</label>
              <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })}
                placeholder="Optional description" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
          </CardContent>
        </Card>

        {/* ── Scope & Environment ──────────────────────── */}
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm">Scope & Environment</CardTitle></CardHeader>
          <CardContent className="space-y-3">
            <div className="grid grid-cols-3 gap-2">
              {([
                { value: 'global', label: 'Global', desc: 'All tenants' },
                { value: 'tenant', label: 'Tenant', desc: 'Single tenant' },
                { value: 'application', label: 'Application', desc: 'App-specific' },
              ] as const).map(({ value, label, desc }) => (
                <button key={value} type="button" onClick={() => setForm({ ...form, scope: value })}
                  className={`rounded-lg border p-2.5 text-center transition-all ${
                    form.scope === value
                      ? 'border-primary bg-primary/10 text-primary shadow-sm'
                      : 'hover:bg-accent'}`}>
                  <div className="text-xs font-medium">{label}</div>
                  <div className="text-[10px] text-muted-foreground">{desc}</div>
                </button>
              ))}
            </div>
            {form.scope !== 'global' && (
              <div>
                <label className="text-xs font-medium">Tenant ID</label>
                <input value={form.tenantId} onChange={(e) => setForm({ ...form, tenantId: e.target.value })}
                  placeholder="Tenant UUID" className="mt-1 w-full rounded-md border px-3 py-2 text-sm font-mono" />
              </div>
            )}
            {form.scope === 'application' && (
              <div>
                <label className="text-xs font-medium">Application Code *</label>
                <input value={form.applicationCode} onChange={(e) => setForm({ ...form, applicationCode: e.target.value })}
                  placeholder="e.g. divipay" className="mt-1 w-full rounded-md border px-3 py-2 text-sm font-mono" />
              </div>
            )}
            <div className="grid grid-cols-2 gap-2">
              {(['sandbox', 'production'] as const).map((env) => (
                <button key={env} type="button" onClick={() => setForm({ ...form, environment: env })}
                  className={`rounded-lg border p-2.5 text-xs font-medium capitalize transition-all ${
                    form.environment === env
                      ? env === 'production'
                        ? 'border-amber-500 bg-amber-50 text-amber-800 shadow-sm'
                        : 'border-primary bg-primary/10 text-primary shadow-sm'
                      : 'hover:bg-accent'}`}>
                  {env} {env === 'production' && <AlertTriangle className="ml-1 inline h-3 w-3" />}
                </button>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* ── Currencies & Limits ──────────────────────── */}
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm">Currencies & Limits</CardTitle></CardHeader>
          <CardContent className="space-y-3">
            <div>
              <label className="text-xs font-medium">Supported Currencies *</label>
              <div className="mt-1 flex flex-wrap gap-1.5">
                {(selectedProvider?.supportedCurrencies || ['IRT', 'IRR']).map((cur) => (
                  <button key={cur} type="button"
                    onClick={() => {
                      const current = form.supportedCurrencies.split(',').map(c => c.trim()).filter(Boolean);
                      const newCurrencies = current.includes(cur)
                        ? current.filter(c => c !== cur)
                        : [...current, cur];
                      setForm({ ...form, supportedCurrencies: newCurrencies.join(', ') });
                    }}
                    className={`rounded-md border px-2.5 py-1 text-xs font-mono transition-all ${
                      form.supportedCurrencies.includes(cur)
                        ? 'border-primary bg-primary/10 text-primary'
                        : 'hover:bg-accent'}`}>
                    {cur}
                  </button>
                ))}
              </div>
            </div>
            <div>
              <label className="text-xs font-medium">Default Currency</label>
              <select value={form.defaultCurrency} onChange={(e) => setForm({ ...form, defaultCurrency: e.target.value })}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm bg-background">
                {form.supportedCurrencies.split(',').map(c => c.trim()).filter(Boolean).map(c => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>
            </div>
            <div className="grid grid-cols-4 gap-3">
              <div>
                <label className="text-xs font-medium">Priority</label>
                <input type="number" value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })}
                  className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
              </div>
              <div>
                <label className="text-xs font-medium">Weight</label>
                <input type="number" value={form.weight} onChange={(e) => setForm({ ...form, weight: e.target.value })}
                  className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
              </div>
              <div>
                <label className="text-xs font-medium">Min Amount</label>
                <input value={form.minAmount} onChange={(e) => setForm({ ...form, minAmount: e.target.value })}
                  placeholder="0" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
              </div>
              <div>
                <label className="text-xs font-medium">Max Amount</label>
                <input value={form.maxAmount} onChange={(e) => setForm({ ...form, maxAmount: e.target.value })}
                  placeholder="No limit" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* ── Credentials (per-provider field defs) ────── */}
        {credentialFields.length > 0 && (
          <Card>
            <CardHeader className="pb-2"><CardTitle className="text-sm">Credentials</CardTitle></CardHeader>
            <CardContent className="space-y-3">
              {credentialFields.map((field) => (
                <SecretInput
                  key={field.key}
                  label={field.label}
                  value={form.secretValues[field.key] || ''}
                  onChange={(v) => setForm({
                    ...form,
                    secretValues: { ...form.secretValues, [field.key]: v }
                  })}
                  placeholder={`Enter ${field.label.toLowerCase()}`}
                />
              ))}
              <p className="text-[10px] text-muted-foreground">
                Credentials are encrypted before storage. Sandbox credentials can be test values.
              </p>
            </CardContent>
          </Card>
        )}

        {/* ── Production warning ───────────────────────── */}
        {form.environment === 'production' && (
          <Card className="border-amber-500 bg-amber-50 dark:bg-amber-950/20">
            <CardContent className="flex items-center gap-3 p-3">
              <AlertTriangle className="h-5 w-5 shrink-0 text-amber-600" />
              <p className="text-sm text-amber-800 dark:text-amber-400">You are creating a PRODUCTION configuration. Real payment credentials will be used. Verify all settings before saving.</p>
            </CardContent>
          </Card>
        )}

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => router.back()}>Cancel</Button>
          <Button type="submit" disabled={mutation.isPending || !form.providerCode}>
            <Save className="mr-1 h-4 w-4" />
            {mutation.isPending ? 'Creating…' : 'Create Config'}
          </Button>
        </div>
      </form>
    </div>
  );
}
