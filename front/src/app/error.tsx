'use client';

import { useEffect } from 'react';
import { AlertTriangle } from 'lucide-react';

interface ErrorPageProps {
  error: Error;
  reset: () => void;
}

export default function GlobalErrorPage({ error, reset }: ErrorPageProps) {
  useEffect(() => {
    console.error('Global error:', error);
  }, [error]);

  return (
    <div className="flex min-h-screen flex-col items-center justify-center space-y-4 p-6">
      <AlertTriangle className="h-16 w-16 text-destructive" />
      <h1 className="text-3xl font-bold">Something Went Wrong</h1>
      <p className="text-muted-foreground max-w-md text-center">
        {error.message || 'An unexpected error occurred. Please try again.'}
      </p>
      <div className="flex gap-4 mt-4">
        <button onClick={reset} className="rounded-md bg-primary px-4 py-2 text-sm text-primary-foreground hover:bg-primary/90">
          Try Again
        </button>
        <a href="/dashboard" className="rounded-md border px-4 py-2 text-sm hover:bg-accent">
          Go to Dashboard
        </a>
      </div>
    </div>
  );
}
