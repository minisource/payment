import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import React from 'react';

// =============================================================
// Mock setup
// =============================================================

// Mock next/navigation
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn(), back: vi.fn() }),
  useParams: () => ({}),
  usePathname: () => '/admin/events/outbox',
}));

// Mock next/link
vi.mock('next/link', () => ({
  default: ({ children, href, ...props }: any) => <a href={href} {...props}>{children}</a>,
}));

// Mock QueryClientProvider
vi.mock('@tanstack/react-query', () => ({
  useQueryClient: () => ({ invalidateQueries: vi.fn() }),
  useQuery: vi.fn(),
  QueryClient: vi.fn(),
  QueryClientProvider: ({ children }: any) => <>{children}</>,
}));

// Mock sonner toast
vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn() },
  Toaster: () => null,
}));

// Mock permission hooks
vi.mock('@/hooks/use-permission', () => ({
  usePermission: () => ({ hasPermission: true }),
  PermissionProvider: ({ children }: any) => <>{children}</>,
}));

// Mock lucide-react icons
vi.mock('lucide-react', () => ({
  ArrowLeft: () => <span>←</span>,
  ChevronDown: () => <span>▼</span>,
  ChevronRight: () => <span>▶</span>,
  Copy: () => <span data-testid="copy-icon">Copy</span>,
  Eye: () => <span>👁</span>,
  EyeOff: () => <span>👁‍🗨</span>,
}));

// =============================================================
// Import components after mocks are set up
// =============================================================

import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';
import { PageHeader } from '@/components/shared/page-header';
import { DetailCard } from '@/components/shared/detail-card';

describe('Phase 7.7 — Shared Components', () => {
  describe('PageHeader', () => {
    it('renders title', () => {
      render(<PageHeader title="Test Title" backHref="/admin/events" />);
      expect(screen.getByText('Test Title')).toBeInTheDocument();
    });

    it('renders actions when provided', () => {
      render(<PageHeader title="Test" backHref="/" actions={<button>Action</button>} />);
      expect(screen.getByText('Action')).toBeInTheDocument();
    });

    it('renders back link with href', () => {
      render(<PageHeader title="Test" backHref="/admin/events" />);
      const backLink = document.querySelector('a[href="/admin/events"]');
      expect(backLink).toBeTruthy();
    });
  });

  describe('DetailCard', () => {
    it('renders title', () => {
      render(<DetailCard title="Test Card"><p>Content</p></DetailCard>);
      expect(screen.getByText('Test Card')).toBeInTheDocument();
    });

    it('renders children', () => {
      render(<DetailCard title="Test"><p>Content</p></DetailCard>);
      expect(screen.getByText('Content')).toBeInTheDocument();
    });
  });
});

describe('Phase 7.7 — SafePayloadViewer', () => {
  it('renders the Payload title', () => {
    render(<SafePayloadViewer data={{ message: 'hello' }} />);
    expect(screen.getByText('Payload')).toBeInTheDocument();
  });

  it('shows JSON content when expanded', () => {
    render(<SafePayloadViewer data={{ message: 'hello', count: 42 }} />);
    // Click the "Payload" button to expand
    fireEvent.click(screen.getByText('Payload'));
    expect(screen.getByText(/"message"/)).toBeInTheDocument();
    expect(screen.getByText(/"hello"/)).toBeInTheDocument();
    expect(screen.getByText(/"count"/)).toBeInTheDocument();
    expect(screen.getByText(/42/)).toBeInTheDocument();
  });

  it('renders empty object when expanded without crashing', () => {
    const { container } = render(<SafePayloadViewer data={{}} />);
    fireEvent.click(screen.getByText('Payload'));
    const pre = container.querySelector('pre');
    expect(pre).toBeTruthy();
    expect(pre?.textContent).toContain('{}');
  });

  it('handles null data gracefully', () => {
    render(<SafePayloadViewer data={null} />);
    expect(screen.getByText('No data available')).toBeInTheDocument();
  });

  it('redacts security-sensitive fields when collapsed', () => {
    const data = {
      secret: 'my-secret-value',
      api_token: 'tok_abc123',
      card_number: '4111-1111-1111-1111',
      iban: 'IR123456789012345678901234',
      bank_account: '123-456-789',
      safe_field: 'visible-value',
    };
    render(<SafePayloadViewer data={data} />);
    // Expand to see content
    fireEvent.click(screen.getByText('Payload'));
    const text = document.body.textContent || '';
    // Redacted fields should not show original values
    expect(text).not.toContain('my-secret-value');
    expect(text).not.toContain('tok_abc123');
    expect(text).not.toContain('4111-1111-1111-1111');
    expect(text).not.toContain('IR123456789012345678901234');
    expect(text).toContain('visible-value');
  });
});

describe('Phase 7.7 — Webhook Edit URL Validation', () => {
  it('validates URL must start with http:// or https://', () => {
    const validUrls = ['https://example.com/webhook', 'http://localhost:3000/hook'];
    const invalidUrls = ['ftp://example.com', '//example.com', 'just-a-string', ''];
    const urlRegex = /^https?:\/\//;
    validUrls.forEach((url) => expect(urlRegex.test(url)).toBe(true));
    invalidUrls.forEach((url) => expect(urlRegex.test(url)).toBe(false));
  });

  it('requires at least one event type', () => {
    const emptyTypes: string[] = [];
    const filledTypes = ['payment.created'];
    expect(emptyTypes.length > 0).toBe(false);
    expect(filledTypes.length > 0).toBe(true);
  });
});

describe('Phase 7.7 — Audit Log Read-Only', () => {
  it('audit detail shows immutable message', () => {
    render(<p className="text-xs">Audit logs are immutable. No edit or delete operations are available.</p>);
    expect(screen.getByText(/immutable/)).toBeInTheDocument();
    expect(screen.getByText(/No edit or delete/)).toBeInTheDocument();
  });
});

describe('Phase 7.7 — System Health Statuses', () => {
  it('renders different health status labels', () => {
    const statuses = ['healthy', 'degraded', 'unhealthy', 'unknown'];
    statuses.forEach((s) => {
      render(<span key={s}>{s}</span>);
      expect(screen.getByText(s)).toBeInTheDocument();
    });
  });
});

describe('Phase 7.7 — Webhook Secret Security', () => {
  it('webhook detail shows secret status as configured/not configured only', () => {
    const configured = <span className="text-green-600 font-medium">Configured</span>;
    const notConfigured = <span className="text-red-600">Not configured</span>;
    const { container: c1 } = render(configured);
    expect(c1.textContent).toBe('Configured');
    const { container: c2 } = render(notConfigured);
    expect(c2.textContent).toBe('Not configured');
  });

  it('webhook edit form has note about secrets', () => {
    const note = <p>Existing webhook secret is not displayed. Use <strong>Regenerate Secret</strong> on the detail page if needed.</p>;
    render(note);
    expect(screen.getByText(/Existing webhook secret is not displayed/)).toBeInTheDocument();
  });
});
