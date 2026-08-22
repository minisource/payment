'use client';

import { useRouter } from 'next/navigation';
import { Shield } from 'lucide-react';

export default function ForbiddenPage() {
  const router = useRouter();

  return (
    <div className="flex min-h-[400px] flex-col items-center justify-center space-y-4">
      <Shield className="h-16 w-16 text-destructive" />
      <h1 className="text-3xl font-bold">403 — Forbidden</h1>
      <p className="text-muted-foreground max-w-md text-center">
        You do not have permission to access this resource. Contact your administrator if you believe this is an error.
      </p>
      <button onClick={() => router.push('/dashboard')} className="mt-4 rounded-md bg-primary px-4 py-2 text-sm text-primary-foreground hover:bg-primary/90">
        Go to Dashboard
      </button>
    </div>
  );
}
