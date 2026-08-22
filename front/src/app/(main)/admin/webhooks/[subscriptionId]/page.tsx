import { redirect } from 'next/navigation';

export default async function WebhookDetailAliasPage({ params }: { params: Promise<{ subscriptionId: string }> }) {
  const { subscriptionId } = await params;
  redirect(`/admin/events/webhooks/${subscriptionId}`);
}
