import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MetricCard } from '@/components/shared/metric-card';

describe('MetricCard', () => {
  it('renders title and value', () => {
    render(<MetricCard title="Test Metric" value="42" />);
    expect(screen.getByText('Test Metric')).toBeDefined();
    expect(screen.getByText('42')).toBeDefined();
  });

  it('renders loading state', () => {
    const { container } = render(<MetricCard title="Loading" isLoading />);
    // Should show skeletons, not the value
    expect(container.querySelector('.animate-pulse')).toBeTruthy();
    expect(screen.queryByText('42')).toBeNull();
  });

  it('renders error state', () => {
    render(<MetricCard title="Error" isError errorMessage="API failed" />);
    expect(screen.getByText('API failed')).toBeDefined();
  });

  it('renders trend indicator', () => {
    render(<MetricCard title="Trend" value="100" trend={{ value: '+12%', direction: 'up' }} />);
    expect(screen.getByText('+12%')).toBeDefined();
  });

  it('renders subtext', () => {
    render(<MetricCard title="Sub" value="5" subtext="Updated 5m ago" />);
    expect(screen.getByText('Updated 5m ago')).toBeDefined();
  });
});
