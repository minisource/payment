import { redirect } from 'next/navigation';

export default function OutboxAliasPage() {
  redirect('/admin/events/outbox');
}
