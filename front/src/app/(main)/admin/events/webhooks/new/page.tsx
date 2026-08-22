'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { PageHeader } from '@/components/shared/page-header';
import { toast } from 'sonner';
import { createWebhookSubscription, type WebhookSecretRegenerateResponse } from '@/api/webhooks';
import { Copy, Shield } from 'lucide-react';

const COMMON_EVENT_TYPES = ['payment.created', 'payment.verified', 'wallet.credited', 'wallet.debited', 'withdrawal.requested', 'withdrawal.completed', 'ledger.entry_created', 'payout_account.verified'];

export default function NewWebhookPage() {
  return (
    <RoutePermissionGuard permissions={['payment.webhook.create']}>
      <NewWebhookContent />
    </RoutePermissionGuard>
  );
}

function NewWebhookContent() {
  const router = useRouter();
  const [form, setForm] = useState({ name: '', targetUrl: '', description: '', selectedTypes: [] as string[] });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [secret, setSecret] = useState<WebhookSecretRegenerateResponse | null>(null);

  const toggleEventType = (type: string) => {
    setForm((f) => ({ ...f, selectedTypes: f.selectedTypes.includes(type) ? f.selectedTypes.filter((t) => t !== type) : [...f.selectedTypes, type] }));
  };

  const handleSubmit = async () => {
    if (!form.name.trim()) { setError('Name is required'); return; }
    if (!form.targetUrl.trim()) { setError('Target URL is required'); return; }
    if (!/^https?:\/\//.test(form.targetUrl)) { setError('Target URL must start with http:// or https://'); return; }
    if (form.selectedTypes.length === 0) { setError('Select at least one event type'); return; }

    setSubmitting(true);
    setError('');
    try {
      const res = await createWebhookSubscription({ name: form.name, targetUrl: form.targetUrl, description: form.description || undefined, eventTypes: form.selectedTypes });
      if (res.secret) {
        setSecret({ subscriptionId: res.id, secret: res.secret, rotatedAt: new Date().toISOString() });
      } else {
        toast.success('Webhook created');
        router.push(`/admin/events/webhooks/${res.id}`);
      }
    } catch (err: any) { setError(err.userMessage || err.message || 'Failed to create webhook'); }
    setSubmitting(false);
  };

  if (secret) {
    return (
      <div className="flex min-h-[400px] flex-col items-center justify-center">
        <div className="w-full max-w-md rounded-lg border bg-background p-6 shadow-lg">
          <div className="flex items-center gap-2 mb-4"><Shield className="h-5 w-5 text-orange-600" /><h2 className="text-lg font-bold">Webhook Created</h2></div>
          <p className="text-sm text-orange-600 mb-4">This secret will not be shown again. Copy and store it securely now.</p>
          <div className="flex items-center gap-2 rounded-md border bg-muted p-3">
            <code className="flex-1 break-all text-xs font-mono">{secret.secret}</code>
            <Button size="sm" variant="ghost" onClick={() => { navigator.clipboard.writeText(secret.secret); toast.success('Secret copied'); }}><Copy className="h-4 w-4" /></Button>
          </div>
          <Button className="mt-4 w-full" onClick={() => router.push(`/admin/events/webhooks/${secret.subscriptionId}`)}>Go to Webhook</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader title="New Webhook" backHref="/admin/events/webhooks" />

      <Card><CardContent className="p-6 space-y-4">
        {error && <div className="rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700">{error}</div>}

        <div>
          <label className="text-sm font-medium">Name *</label>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="My Webhook" className="mt-1" />
        </div>

        <div>
          <label className="text-sm font-medium">Target URL *</label>
          <Input value={form.targetUrl} onChange={(e) => setForm({ ...form, targetUrl: e.target.value })} placeholder="https://example.com/webhook" className="mt-1 font-mono text-sm" />
        </div>

        <div>
          <label className="text-sm font-medium">Description</label>
          <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Optional description" className="mt-1" />
        </div>

        <div>
          <label className="text-sm font-medium">Event Types *</label>
          <p className="text-xs text-muted-foreground mb-2">Select at least one event type this webhook should receive.</p>
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
          <Button onClick={handleSubmit} disabled={submitting}>{submitting ? 'Creating...' : 'Create Webhook'}</Button>
          <Button variant="outline" onClick={() => router.back()}>Cancel</Button>
        </div>
      </CardContent></Card>
    </div>
  );
}
