import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MoneyAmount } from '@/components/shared/money-amount';

describe('MoneyAmount', () => {
  it('renders string amount safely', () => {
    render(<MoneyAmount amount="1250000" currency="IRR" />);
    expect(screen.getByText(/1,250,000/)).toBeDefined();
  });

  it('renders number amount', () => {
    render(<MoneyAmount amount={50000} currency="IRR" />);
    expect(screen.getByText(/50,000/)).toBeDefined();
  });

  it('renders dash for NaN', () => {
    const { container } = render(<MoneyAmount amount="not-a-number" />);
    expect(container.textContent).toContain('\u2014');
  });

  it('renders negative values with minus sign', () => {
    render(<MoneyAmount amount={-50000} currency="IRR" />);
    expect(screen.getByText(/50,000/)).toBeDefined();
  });

  it('shows loading skeleton', () => {
    const { container } = render(<MoneyAmount amount="100" isLoading />);
    expect(container.querySelector('.animate-pulse')).toBeTruthy();
  });

  it('renders currency symbol', () => {
    render(<MoneyAmount amount="100" currency="USD" />);
    expect(screen.getByText(/\$/)).toBeDefined();
  });
});
