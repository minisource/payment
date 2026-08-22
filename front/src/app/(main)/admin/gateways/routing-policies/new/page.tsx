'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { toast } from 'sonner';
import { createRoutingPolicy, type RoutingPolicyCreateRequest, type RoutingRule } from '@/api/gateways';
import { ArrowLeft, Save, Plus, Trash2 } from 'lucide-react';

export default function NewRoutingPolicyPage() {
  return (
    <RoutePermissionGuard permissions={['payment.gateway.routing.manage']}>
      <NewPolicyForm />
    </RoutePermissionGuard>
  );
}

function NewPolicyForm() {
  const router = useRouter();
  const [form, setForm] = useState({
    name: '', description: '',
    scope: 'tenant' as 'global' | 'tenant' | 'application',
    strategy: 'priority' as 'priority' | 'random' | 'weighted_random',
    fallbackEnabled: true,
    status: 'active',
  });
  const [rules, setRules] = useState<RoutingRule[]>([
    { providerCode: '', gatewayConfigId: '', priority: 1, weight: 1, currency: '', minAmount: undefined, maxAmount: undefined, conditions: {}, status: 'active' },
  ]);

  const mutation = useMutation({
    mutationFn: (req: RoutingPolicyCreateRequest) => createRoutingPolicy(req),
    onSuccess: (data) => { toast.success('Policy created'); router.push(`/admin/gateways/routing-policies/${data.id}`); },
    onError: (err: Error) => toast.error(err.message),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name) { toast.error('Name is required'); return; }
    const validRules = rules.filter(r => r.providerCode && r.gatewayConfigId);
    if (!validRules.length) { toast.error('At least one rule with provider and config is required'); return; }

    // Validate strategy-specific requirements
    if (form.strategy === 'priority') {
      const noPriority = validRules.some(r => !r.priority);
      if (noPriority) { toast.error('All rules must have a priority for priority strategy'); return; }
    }
    if (form.strategy === 'weighted_random') {
      const noWeight = validRules.some(r => !r.weight || r.weight <= 0);
      if (noWeight) { toast.error('All rules must have positive weight for weighted_random'); return; }
    }
    if (validRules.some(r => r.minAmount && r.maxAmount && Number(r.minAmount) >= Number(r.maxAmount))) {
      toast.error('Min amount must be less than max amount'); return;
    }

    mutation.mutate({ ...form, rules: validRules });
  };

  const addRule = () => {
    setRules([...rules, { providerCode: '', gatewayConfigId: '', priority: rules.length + 1, weight: 1, currency: '', status: 'active' }]);
  };

  const updateRule = (idx: number, updates: Partial<RoutingRule>) => {
    setRules(rules.map((r, i) => i === idx ? { ...r, ...updates } : r));
  };

  const removeRule = (idx: number) => {
    if (rules.length <= 1) return;
    setRules(rules.filter((_, i) => i !== idx));
  };

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-3">
        <button onClick={() => router.back()} className="text-muted-foreground hover:text-foreground"><ArrowLeft className="h-5 w-5" /></button>
        <h1 className="text-2xl font-bold">New Routing Policy</h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader><CardTitle className="text-sm">Basic Info</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div><label className="text-xs font-medium">Name *</label>
              <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="e.g. Production Routing" className="mt-1 w-full rounded-md border px-3 py-2 text-sm" required />
            </div>
            <div><label className="text-xs font-medium">Description</label>
              <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Strategy</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-3 gap-2">
              {(['priority', 'random', 'weighted_random'] as const).map((s) => (
                <button key={s} type="button" onClick={() => setForm({ ...form, strategy: s })}
                  className={`rounded-md border px-3 py-2 text-xs font-medium ${form.strategy === s ? 'border-primary bg-primary/10 text-primary' : 'hover:bg-accent'}`}>
                  {s === 'weighted_random' ? 'Weighted' : s === 'priority' ? 'Priority' : 'Random'}
                </button>
              ))}
            </div>
            <div className="flex items-center gap-2">
              <input type="checkbox" checked={form.fallbackEnabled} onChange={(e) => setForm({ ...form, fallbackEnabled: e.target.checked })} />
              <label className="text-xs">Enable fallback (route to any available config if no rules match)</label>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-sm">Routing Rules</CardTitle>
            <Button type="button" variant="outline" size="sm" onClick={addRule}><Plus className="mr-1 h-3 w-3" /> Add Rule</Button>
          </CardHeader>
          <CardContent className="space-y-4">
            {rules.map((rule, idx) => (
              <div key={idx} className="rounded-md border p-4 space-y-3 relative">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-medium text-muted-foreground">Rule {idx + 1}</span>
                  {rules.length > 1 && (
                    <button type="button" onClick={() => removeRule(idx)} className="text-red-500 hover:text-red-700"><Trash2 className="h-4 w-4" /></button>
                  )}
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <div><label className="text-[10px] font-medium">Provider Code *</label>
                    <input value={rule.providerCode} onChange={(e) => updateRule(idx, { providerCode: e.target.value })} placeholder="e.g. zarinpal" className="mt-0.5 w-full rounded-md border px-2 py-1.5 text-xs font-mono" />
                  </div>
                  <div><label className="text-[10px] font-medium">Gateway Config ID *</label>
                    <input value={rule.gatewayConfigId} onChange={(e) => updateRule(idx, { gatewayConfigId: e.target.value })} placeholder="UUID" className="mt-0.5 w-full rounded-md border px-2 py-1.5 text-xs font-mono" />
                  </div>
                  <div><label className="text-[10px] font-medium">Priority{form.strategy === 'priority' ? ' *' : ''}</label>
                    <input type="number" value={rule.priority} onChange={(e) => updateRule(idx, { priority: Number(e.target.value) })} className="mt-0.5 w-full rounded-md border px-2 py-1.5 text-xs" />
                  </div>
                  <div><label className="text-[10px] font-medium">Weight{form.strategy === 'weighted_random' ? ' *' : ''}</label>
                    <input type="number" value={rule.weight} onChange={(e) => updateRule(idx, { weight: Number(e.target.value) })} className="mt-0.5 w-full rounded-md border px-2 py-1.5 text-xs" />
                  </div>
                  <div><label className="text-[10px] font-medium">Currency (optional)</label>
                    <input value={rule.currency || ''} onChange={(e) => updateRule(idx, { currency: e.target.value || undefined })} placeholder="IRT" className="mt-0.5 w-full rounded-md border px-2 py-1.5 text-xs font-mono" />
                  </div>
                  <div />
                  <div><label className="text-[10px] font-medium">Min Amount</label>
                    <input value={rule.minAmount || ''} onChange={(e) => updateRule(idx, { minAmount: e.target.value ? Number(e.target.value) : undefined })} placeholder="0" className="mt-0.5 w-full rounded-md border px-2 py-1.5 text-xs" />
                  </div>
                  <div><label className="text-[10px] font-medium">Max Amount</label>
                    <input value={rule.maxAmount || ''} onChange={(e) => updateRule(idx, { maxAmount: e.target.value ? Number(e.target.value) : undefined })} placeholder="No limit" className="mt-0.5 w-full rounded-md border px-2 py-1.5 text-xs" />
                  </div>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => router.back()}>Cancel</Button>
          <Button type="submit" disabled={mutation.isPending}><Save className="mr-1 h-4 w-4" /> {mutation.isPending ? 'Creating…' : 'Create Policy'}</Button>
        </div>
      </form>
    </div>
  );
}
