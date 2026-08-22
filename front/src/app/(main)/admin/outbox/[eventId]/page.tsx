import { redirect } from 'next/navigation';

export default async function OutboxEventAliasPage({ params }: { params: Promise<{ eventId: string }> }) {
  const { eventId } = await params;
  redirect(`/admin/events/outbox/${eventId}`);
}
