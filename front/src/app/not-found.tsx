'use client';

import { useRouter } from 'next/navigation';
import { FileQuestion } from 'lucide-react';

export default function NotFoundPage() {
  const router = useRouter();

  return (
    <div className="flex min-h-[400px] flex-col items-center justify-center space-y-4">
      <FileQuestion className="h-16 w-16 text-muted-foreground" />
      <h1 className="text-3xl font-bold">404 — Not Found</h1>
      <p className="text-muted-foreground max-w-md text-center">The resource you are looking for does not exist or may have been removed.</p>
      <button onClick={() => router.push('/dashboard')} className="mt-4 rounded-md bg-primary px-4 py-2 text-sm text-primary-foreground hover:bg-primary/90">
        Go to Dashboard
      </button>
    </div>
  );
}
