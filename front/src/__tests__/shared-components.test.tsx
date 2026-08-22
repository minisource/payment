import { describe, it, expect, vi } from 'vitest';
import { MoneyAmount, CopyText, StatusBadge } from '@/components/shared/components';
import { render, screen, fireEvent } from '@testing-library/react';

describe('MoneyAmount', () => {
  it('renders amount with currency', () => {
    render(<MoneyAmount amount={1000000} currency="IRR" />);
    expect(screen.getByText('1,000,000 IRR')).toBeInTheDocument();
  });

  it('renders string amounts', () => {
    render(<MoneyAmount amount="5000" currency="USD" />);
    expect(screen.getByText('5,000 USD')).toBeInTheDocument();
  });

  it('renders dash for invalid amounts', () => {
    render(<MoneyAmount amount="invalid" currency="IRR" />);
    expect(screen.getByText('—')).toBeInTheDocument();
  });
});

describe('StatusBadge', () => {
  it('renders active status in green', () => {
    render(<StatusBadge status="active" />);
    const badge = screen.getByText('active');
    expect(badge).toBeInTheDocument();
    expect(badge.className).toContain('bg-green');
  });

  it('renders failed status in red', () => {
    render(<StatusBadge status="failed" />);
    const badge = screen.getByText('failed');
    expect(badge.className).toContain('bg-red');
  });

  it('replaces underscores with spaces', () => {
    render(<StatusBadge status="dead_lettered" />);
    expect(screen.getByText('dead lettered')).toBeInTheDocument();
  });
});

describe('CopyText', () => {
  it('renders text value', () => {
    render(<CopyText text="abc123" />);
    expect(screen.getByText('abc123')).toBeInTheDocument();
  });

  it('copies text on click', async () => {
    const writeText = vi.fn();
    Object.defineProperty(navigator, 'clipboard', {
      value: { writeText },
      writable: true,
    });

    render(<CopyText text="secret" />);
    fireEvent.click(screen.getByText('secret'));
    expect(writeText).toHaveBeenCalledWith('secret');
  });
});
