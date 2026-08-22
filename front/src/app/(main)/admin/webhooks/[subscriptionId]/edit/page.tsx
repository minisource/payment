import { redirect } from 'next/navigation';

export default async function WebhookEditAliasPage({ params }: { params: Promise<{ subscriptionId: string }> }) {
  const { subscriptionId } = await params;
  redirect(`/admin/events/webhooks/${subscriptionId}/edit`);
}
