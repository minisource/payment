'use client';

import { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { SecretInput } from '@/components/shared/secret-input';
import { Loading, ErrorState } from '@/components/shared/states';
import { toast } from 'sonner';
import { getGatewayConfig, updateGatewayConfig, type GatewayConfigUpdateRequest } from '@/api/gateways';
import { ArrowLeft, Save, AlertTriangle } from 'lucide-react';

export default function EditGatewayConfigPage() {
  const { gatewayConfigId } = useParams<{ gatewayConfigId: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();

  const { data: config, isLoading, isError, error } = useQuery({
    queryKey: ['gateway-config', gatewayConfigId],
    queryFn: () => getGatewayConfig(gatewayConfigId),
  });

  const [form, setForm] = useState({
    name: '', description: '', providerCode: '',
    scope: 'tenant' as 'global' | 'tenant' | 'application',
    environment: 'sandbox' as 'sandbox' | 'production',
    priority: '', weight: '', minAmount: '', maxAmount: '',
    supportedCurrencies: '', defaultCurrency: '',
    secretValues: {} as Record<string, string>,
  });

  useEffect(() => {
    if (config) {
      setForm({
        name: config.name, description: config.description || '',
        providerCode: config.providerCode, scope: config.scope,
        environment: config.environment,
        priority: String(config.priority), weight: String(config.weight),
        minAmount: config.minAmount != null ? String(config.minAmount) : '',
        maxAmount: config.maxAmount != null ? String(config.maxAmount) : '',
        supportedCurrencies: config.supportedCurrencies?.join(', ') || '',
        defaultCurrency: config.defaultCurrency || '',
        secretValues: {},
      });
    }
  }, [config]);

  const mutation = useMutation({
    mutationFn: (req: GatewayConfigUpdateRequest) => updateGatewayConfig(gatewayConfigId, req),
    onSuccess: () => { toast.success('Config updated'); queryClient.invalidateQueries({ queryKey: ['gateway-config', gatewayConfigId] }); },
    onError: (err: Error) => toast.error(err.message),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name) { toast.error('Name is required'); return; }
    const _req: GatewayConfigUpdateRequest = {
      name: form.name, description: form.description || undefined,
      scope: form.scope, environment: form.environment,
      supportedCurrencies: form.supportedCurrencies.split(',').map(c => c.trim()).filter(Boolean),
      defaultCurrency: form.defaultCurrency || undefined,
      priority: form.priority ? parseInt(form.priority) : undefined,
      weight: form.weight ? parseInt(form.weight) : undefined,
      minAmount: form.minAmount || undefined, maxAmount: form.maxAmount || undefined,
      secretValues: Object.keys(form.secretValues).length ? form.secretValues : undefined,
    };
    mutation.mutate(_req);
  };

  if (isLoading) return <Loading />;
  if (isError || !config) return <ErrorState error={(error as Error)?.message} />;

  const isProduction = config.environment === 'production';
  const secretFields = config.secretFieldsStatus || [];

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="flex items-center gap-3">
        <button onClick={() => router.back()} className="text-muted-foreground hover:text-foreground"><ArrowLeft className="h-5 w-5" /></button>
        <h1 className="text-2xl font-bold">Edit Gateway Config</h1>
      </div>

      {isProduction && (
        <Card className="border-amber-500 bg-amber-50 dark:bg-amber-950/20">
          <CardContent className="flex items-center gap-3 p-3">
            <AlertTriangle className="h-5 w-5 text-amber-600" />
            <p className="text-sm text-amber-800 dark:text-amber-400">This is a PRODUCTION configuration. Changes may affect live payments.</p>
          </CardContent>
        </Card>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader><CardTitle className="text-sm">Basic Info</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div><label className="text-xs font-medium">Name *</label>
              <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" required />
            </div>
            <div><label className="text-xs font-medium">Description</label>
              <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Environment</CardTitle></CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-2">
              {(['sandbox', 'production'] as const).map((env) => (
                <button key={env} type="button" onClick={() => setForm({ ...form, environment: env })}
                  className={`rounded-md border px-3 py-2 text-xs font-medium capitalize ${form.environment === env ? env === 'production' ? 'border-amber-500 bg-amber-50 text-amber-800' : 'border-primary bg-primary/10 text-primary' : 'hover:bg-accent'}`}>
                  {env}
                </button>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Currencies & Limits</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div><label className="text-xs font-medium">Supported Currencies</label>
              <input value={form.supportedCurrencies} onChange={(e) => setForm({ ...form, supportedCurrencies: e.target.value })} className="mt-1 w-full rounded-md border px-3 py-2 text-sm font-mono" />
            </div>
            <div className="grid grid-cols-3 gap-3">
              <div><label className="text-xs font-medium">Priority</label><input type="number" value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>
              <div><label className="text-xs font-medium">Weight</label><input type="number" value={form.weight} onChange={(e) => setForm({ ...form, weight: e.target.value })} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>
              <div />
              <div><label className="text-xs font-medium">Min Amount</label><input value={form.minAmount} onChange={(e) => setForm({ ...form, minAmount: e.target.value })} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>
              <div><label className="text-xs font-medium">Max Amount</label><input value={form.maxAmount} onChange={(e) => setForm({ ...form, maxAmount: e.target.value })} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Secrets (blank = unchanged)</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            {secretFields.map((sf) => (
              <SecretInput key={sf.key} label={sf.key}
                value={form.secretValues[sf.key] || ''}
                onChange={(v) => setForm({ ...form, secretValues: { ...form.secretValues, [sf.key]: v } })}
                existing={sf.configured}
              />
            ))}
            {secretFields.length === 0 && <p className="text-sm text-muted-foreground">No secret fields for this provider</p>}
          </CardContent>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => router.back()}>Cancel</Button>
          <Button type="submit" disabled={mutation.isPending}><Save className="mr-1 h-4 w-4" /> {mutation.isPending ? 'Saving…' : 'Save Changes'}</Button>
        </div>
      </form>
    </div>
  );
}
