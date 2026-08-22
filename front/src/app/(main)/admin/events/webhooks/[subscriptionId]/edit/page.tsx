'use client';

import { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { Loading, ErrorState } from '@/components/shared/states';
import { PageHeader } from '@/components/shared/page-header';
import { toast } from 'sonner';
import { getWebhookSubscription, updateWebhookSubscription } from '@/api/webhooks';

const COMMON_EVENT_TYPES = ['payment.created', 'payment.verified', 'wallet.credited', 'wallet.debited', 'withdrawal.requested', 'withdrawal.completed', 'ledger.entry_created', 'payout_account.verified'];

export default function EditWebhookPage() {
  return (
    <RoutePermissionGuard permissions={['payment.webhook.update']}>
      <EditWebhookContent />
    </RoutePermissionGuard>
  );
}

function EditWebhookContent() {
  const { subscriptionId } = useParams<{ subscriptionId: string }>();
  const router = useRouter();
  const [form, setForm] = useState({ name: '', targetUrl: '', description: '', selectedTypes: [] as string[] });
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState('');

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['webhook', subscriptionId],
    queryFn: () => getWebhookSubscription(subscriptionId as string),
    enabled: !!subscriptionId,
  });

  useEffect(() => {
    if (data) {
      setForm({
        name: data.name,
        targetUrl: data.targetUrl,
        description: data.description || '',
        selectedTypes: data.eventTypes,
      });
    }
  }, [data]);

  const toggleEventType = (type: string) => {
    setForm((f) => ({ ...f, selectedTypes: f.selectedTypes.includes(type) ? f.selectedTypes.filter((t) => t !== type) : [...f.selectedTypes, type] }));
  };

  const handleSubmit = async () => {
    const name = form.name.trim();
    const targetUrl = form.targetUrl.trim();

    if (!name) { setFormError('Name is required'); return; }
    if (!targetUrl) { setFormError('Target URL is required'); return; }
    if (!/^https?:\/\//.test(targetUrl)) { setFormError('Target URL must start with http:// or https://'); return; }
    if (form.selectedTypes.length === 0) { setFormError('Select at least one event type'); return; }

    setSubmitting(true);
    setFormError('');
    try {
      await updateWebhookSubscription(subscriptionId as string, {
        name,
        targetUrl,
        description: form.description || undefined,
        eventTypes: form.selectedTypes,
      });
      toast.success('Webhook updated');
      router.push(`/admin/events/webhooks/${subscriptionId}`);
    } catch (err: any) { setFormError(err.userMessage || err.message || 'Failed to update webhook'); }
    setSubmitting(false);
  };

  if (isLoading) return <Loading />;
  if (isError || !data) return <ErrorState error={(error as Error)?.message} onRetry={() => router.push('/admin/events/webhooks')} />;

  return (
    <div className="space-y-6">
      <PageHeader title={`Edit: ${data.name}`} backHref={`/admin/events/webhooks/${subscriptionId}`} />

      <Card><CardContent className="p-6 space-y-4">
        {formError && <div className="rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700">{formError}</div>}

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

        <div className="rounded-md border border-yellow-200 bg-yellow-50 p-3 text-sm text-yellow-800">
          <p className="font-medium">Note on secrets</p>
          <p className="mt-1 text-xs">Existing webhook secret is not displayed. Use <strong>Regenerate Secret</strong> on the detail page if needed.</p>
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
          <Button onClick={handleSubmit} disabled={submitting}>{submitting ? 'Saving...' : 'Save Changes'}</Button>
          <Button variant="outline" onClick={() => router.push(`/admin/events/webhooks/${subscriptionId}`)}>Cancel</Button>
        </div>
      </CardContent></Card>
    </div>
  );
}
