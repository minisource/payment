import { redirect } from 'next/navigation';

export default function WebhookNewAliasPage() {
  redirect('/admin/events/webhooks/new');
}
