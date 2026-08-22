import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Providers } from '@/components/providers';

describe('App renders', () => {
  it('renders providers without crashing', () => {
    render(
      <Providers>
        <div>Hello World</div>
      </Providers>,
    );
    expect(screen.getByText('Hello World')).toBeInTheDocument();
  });

  it('renders children inside providers', () => {
    render(
      <Providers>
        <span data-testid="child">Test Child</span>
      </Providers>,
    );
    expect(screen.getByTestId('child')).toBeInTheDocument();
  });
});
