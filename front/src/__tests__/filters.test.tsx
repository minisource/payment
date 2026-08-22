import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import React from 'react';
import { DateRangeFilter, SearchInput, RefreshButton } from '@/components/shared/filters';

describe('DateRangeFilter', () => {
  it('renders date inputs', () => {
    render(<DateRangeFilter onChange={() => {}} />);
    const inputs = document.querySelectorAll('input[type="date"]');
    expect(inputs.length).toBe(2);
  });

  it('calls onChange when date is selected', () => {
    const onChange = vi.fn();
    render(<DateRangeFilter onChange={onChange} />);
    const inputs = document.querySelectorAll('input[type="date"]');
    fireEvent.change(inputs[0], { target: { value: '2026-01-01' } });
    expect(onChange).toHaveBeenCalled();
  });

  it('shows clear button when dates are set', () => {
    render(<DateRangeFilter dateFrom="2026-01-01" dateTo="2026-01-31" onChange={() => {}} />);
    // Should have an X button
    const buttons = document.querySelectorAll('button');
    expect(buttons.length).toBeGreaterThan(0);
  });
});

describe('SearchInput', () => {
  it('renders search input', () => {
    render(<SearchInput value="" onChange={() => {}} />);
    expect(screen.getByPlaceholderText('Search...')).toBeDefined();
  });

  it('renders with custom placeholder', () => {
    render(<SearchInput value="" onChange={() => {}} placeholder="Find wallets..." />);
    expect(screen.getByPlaceholderText('Find wallets...')).toBeDefined();
  });

  it('shows clear button when value present', () => {
    render(<SearchInput value="test" onChange={() => {}} />);
    const buttons = document.querySelectorAll('button');
    expect(buttons.length).toBeGreaterThan(0);
  });
});

describe('RefreshButton', () => {
  it('renders refresh button', () => {
    render(<RefreshButton onClick={() => {}} />);
    expect(screen.getByText('Refresh')).toBeDefined();
  });

  it('shows last refreshed time', () => {
    const date = new Date('2026-01-01T12:00:00');
    render(<RefreshButton onClick={() => {}} lastRefreshed={date} />);
    expect(screen.getByText(/Last:/)).toBeDefined();
  });
});
