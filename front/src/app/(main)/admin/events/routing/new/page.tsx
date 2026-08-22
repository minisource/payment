'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { PageHeader } from '@/components/shared/page-header';
import { toast } from 'sonner';
import { createEventRoutingRule, type EventTargetType } from '@/api/event-routing';

const COMMON_EVENT_TYPES = ['payment.created', 'payment.verified', 'wallet.credited', 'wallet.debited', 'withdrawal.requested', 'withdrawal.completed', 'ledger.entry_created', 'payout_account.verified'];
const TARGET_TYPES: { value: EventTargetType; label: string }[] = [
  { value: 'webhook', label: 'Webhook' },
  { value: 'notifier', label: 'Notifier' },
  { value: 'message_bus', label: 'Message Bus' },
  { value: 'internal_handler', label: 'Internal Handler' },
];

export default function NewEventRoutingRulePage() {
  return (
    <RoutePermissionGuard permissions={['payment.event_routing.create']}>
      <NewRoutingContent />
    </RoutePermissionGuard>
  );
}

function NewRoutingContent() {
  const router = useRouter();
  const [form, setForm] = useState({ name: '', description: '', selectedTypes: [] as string[], targetType: 'webhook' as EventTargetType, targetConfig: '', priority: 0 });
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState('');

  const toggleEventType = (type: string) => {
    setForm((f) => ({ ...f, selectedTypes: f.selectedTypes.includes(type) ? f.selectedTypes.filter((t) => t !== type) : [...f.selectedTypes, type] }));
  };

  const handleSubmit = async () => {
    if (!form.name.trim()) { setFormError('Name is required'); return; }
    if (form.selectedTypes.length === 0) { setFormError('Select at least one event type'); return; }

    let targetConfig: Record<string, unknown> = {};
    if (form.targetConfig.trim()) {
      try { targetConfig = JSON.parse(form.targetConfig); } catch { setFormError('Target config must be valid JSON'); return; }
    }

    setSubmitting(true);
    setFormError('');
    try {
      const res = await createEventRoutingRule({
        name: form.name,
        description: form.description || undefined,
        eventTypes: form.selectedTypes,
        targetType: form.targetType,
        targetConfig,
        priority: form.priority || undefined,
      });
      toast.success('Routing rule created');
      router.push(`/admin/events/routing/${res.id}`);
    } catch (err: any) { setFormError(err.userMessage || err.message || 'Failed to create rule'); }
    setSubmitting(false);
  };

  return (
    <div className="space-y-6">
      <PageHeader title="New Event Routing Rule" backHref="/admin/events/routing" />

      <Card><CardContent className="p-6 space-y-4">
        {formError && <div className="rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700">{formError}</div>}

        <div>
          <label className="text-sm font-medium">Name *</label>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="Payment events to webhook" className="mt-1" />
        </div>

        <div>
          <label className="text-sm font-medium">Description</label>
          <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Optional description" className="mt-1" />
        </div>

        <div>
          <label className="text-sm font-medium">Target Type *</label>
          <select value={form.targetType} onChange={(e) => setForm({ ...form, targetType: e.target.value as EventTargetType })} className="mt-1 h-9 w-full rounded-md border bg-background px-3 text-sm">
            {TARGET_TYPES.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
          </select>
        </div>

        <div>
          <label className="text-sm font-medium">Priority</label>
          <Input type="number" value={form.priority} onChange={(e) => setForm({ ...form, priority: parseInt(e.target.value) || 0 })} className="mt-1 w-32" />
        </div>

        <div>
          <label className="text-sm font-medium">Target Config (JSON)</label>
          <textarea value={form.targetConfig} onChange={(e) => setForm({ ...form, targetConfig: e.target.value })} placeholder='{"url": "https://...", ...}' className="mt-1 w-full rounded-md border bg-background px-3 py-2 font-mono text-xs min-h-[80px]" />
        </div>

        <div>
          <label className="text-sm font-medium">Event Types *</label>
          <p className="text-xs text-muted-foreground mb-2">Select at least one event type to route.</p>
          <div className="grid gap-2 sm:grid-cols-2">
            {COMMON_EVENT_TYPES.map((type) => (
              <label key={type} className={`flex items-center gap-2 rounded-md border p-2 cursor-pointer transition-colors text-xs ${form.selectedTypes.includes(type) ? 'border-primary bg-primary/5' : 'hover:border-muted-foreground/30'}`}>
                <input type="checkbox" checked={form.selectedTypes.includes(type)} onChange={() => toggleEventType(type)} className="h-4 w-4" />
                <code className="text-xs">{type}</code>
              </label>
            ))}
          </div>
        </div>

        <div className="flex gap-2 pt-2">
          <Button onClick={handleSubmit} disabled={submitting}>{submitting ? 'Creating...' : 'Create Rule'}</Button>
          <Button variant="outline" onClick={() => router.back()}>Cancel</Button>
        </div>
      </CardContent></Card>
    </div>
  );
}
