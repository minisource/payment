'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { ConfirmDialog } from '@/components/shared/components';
import { toast } from 'sonner';
import { getPaymentLink, pausePaymentLink, resumePaymentLink, disablePaymentLink } from '@/api/payment-links';
import { Pause, Play, Ban, Copy } from 'lucide-react';

export default function PaymentLinkDetailPage() {
  const { paymentLinkId } = useParams<{ paymentLinkId: string }>();
  const queryClient = useQueryClient();
  const [action, setAction] = useState<'pause' | 'resume' | 'disable' | null>(null);

  const { data: link, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['payment-link', paymentLinkId],
    queryFn: () => getPaymentLink(paymentLinkId),
  });

  const actionMutation = useMutation({
    mutationFn: (act: 'pause' | 'resume' | 'disable') =>
      act === 'pause' ? pausePaymentLink(paymentLinkId) : act === 'resume' ? resumePaymentLink(paymentLinkId) : disablePaymentLink(paymentLinkId),
    onSuccess: () => { toast.success('Action completed'); queryClient.invalidateQueries({ queryKey: ['payment-link', paymentLinkId] }); setAction(null); },
    onError: (err: Error) => toast.error(err.message),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !link) return <ErrorState error={(error as Error)?.message || 'Not found'} onRetry={() => refetch()} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <Link href="/admin/payment-links" className="text-sm text-muted-foreground hover:text-foreground">← Payment Links</Link>
          <div className="mt-1 flex items-center gap-3"><h1 className="text-xl font-bold">{link.title}</h1><StatusBadge status={link.status} /></div>
        </div>
        <div className="flex gap-2">
          {link.safePublicUrl && <Button variant="outline" size="sm" onClick={() => { navigator.clipboard.writeText(link.safePublicUrl!); toast.success('URL copied'); }}><Copy className="mr-1 h-3.5 w-3.5" /> Copy URL</Button>}
          {link.status === 'active' && <Button variant="outline" size="sm" onClick={() => setAction('pause')}><Pause className="mr-1 h-3.5 w-3.5" /> Pause</Button>}
          {link.status === 'paused' && <Button variant="outline" size="sm" onClick={() => setAction('resume')}><Play className="mr-1 h-3.5 w-3.5" /> Resume</Button>}
          {link.status !== 'disabled' && link.status !== 'deleted' && <Button variant="outline" size="sm" onClick={() => setAction('disable')} className="text-red-600"><Ban className="mr-1 h-3.5 w-3.5" /> Disable</Button>}
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">ID</span><span className="font-mono text-xs">{link.id}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Owner</span><span className="text-xs">{link.ownerType}: {link.ownerId}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Amount Type</span><span className="text-xs capitalize">{link.amountType}</span></div>
            {link.amount != null && <div className="flex justify-between"><span className="text-muted-foreground">Amount</span><MoneyAmount amount={link.amount} currency={link.currency} /></div>}
            <div className="flex justify-between"><span className="text-muted-foreground">Currency</span><span className="text-xs">{link.currency}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Uses</span><span className="text-xs">{link.usedCount}{link.maxUses ? ` / ${link.maxUses}` : ''}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Total Paid</span><MoneyAmount amount={link.totalPaid} currency={link.currency} /></div>
            {link.expiresAt && <div className="flex justify-between"><span className="text-muted-foreground">Expires</span><span className="text-xs">{new Date(link.expiresAt).toLocaleString()}</span></div>}
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span className="text-xs">{new Date(link.createdAt).toLocaleString()}</span></div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Public URL</CardTitle></CardHeader>
          <CardContent>
            {link.safePublicUrl ? (
              <div className="flex items-center gap-2"><span className="font-mono text-xs break-all">{link.safePublicUrl}</span><Button variant="ghost" size="sm" onClick={() => { navigator.clipboard.writeText(link.safePublicUrl!); toast.success('Copied'); }}><Copy className="h-3.5 w-3.5" /></Button></div>
            ) : <p className="text-sm text-muted-foreground">No public URL available</p>}
          </CardContent>
        </Card>
      </div>

      {link.description && <Card><CardContent className="p-4"><p className="text-sm text-muted-foreground">{link.description}</p></CardContent></Card>}

      {link.metadata && <SafePayloadViewer data={link.metadata} title="Metadata" />}

      {action && (
        <ConfirmDialog open onClose={() => setAction(null)} onConfirm={() => actionMutation.mutate(action!)}
          title={`${action === 'pause' ? 'Pause' : action === 'resume' ? 'Resume' : 'Disable'} Payment Link`}
          confirmLabel={action === 'pause' ? 'Pause' : action === 'resume' ? 'Resume' : 'Disable'} destructive={action === 'disable'} />
      )}
    </div>
  );
}
