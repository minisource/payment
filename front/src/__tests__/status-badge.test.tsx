import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { StatusBadge, RiskSeverityBadge } from '@/components/shared/enhanced-badge';

describe('StatusBadge', () => {
  it('renders known status correctly', () => {
    render(<StatusBadge status="pending" />);
    expect(screen.getByText('pending')).toBeDefined();
  });

  it('renders unknown status safely', () => {
    render(<StatusBadge status="some_unknown_status" />);
    expect(screen.getByText('some unknown status')).toBeDefined();
  });

  it('renders with underscores replaced by spaces', () => {
    render(<StatusBadge status="dead_lettered" />);
    expect(screen.getByText('dead lettered')).toBeDefined();
  });

  it('handles succeeded status', () => {
    const { container } = render(<StatusBadge status="succeeded" />);
    expect(container.querySelector('.bg-green-100')).toBeTruthy();
  });

  it('handles failed status', () => {
    const { container } = render(<StatusBadge status="failed" />);
    expect(container.querySelector('.bg-red-100')).toBeTruthy();
  });
});

describe('RiskSeverityBadge', () => {
  it('renders low severity', () => {
    render(<RiskSeverityBadge severity="low" />);
    expect(screen.getByText('low')).toBeDefined();
  });

  it('renders critical severity with red', () => {
    const { container } = render(<RiskSeverityBadge severity="critical" />);
    expect(container.querySelector('.bg-red-100')).toBeTruthy();
  });

  it('renders high severity', () => {
    render(<RiskSeverityBadge severity="high" />);
    expect(screen.getByText('high')).toBeDefined();
  });
});
