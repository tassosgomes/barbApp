import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { LoadingSpinner, LoadingState } from './Loading';

describe('LoadingSpinner', () => {
  it('should render with default size (md)', () => {
    const { container } = render(<LoadingSpinner />);

    const spinner = container.firstChild as HTMLElement;
    expect(spinner).toHaveClass('animate-spin');
    expect(spinner).toHaveClass('h-8');
    expect(spinner).toHaveClass('w-8');
    expect(spinner).toHaveClass('border-t-2');
    expect(spinner).toHaveClass('border-b-2');
    expect(spinner).toHaveClass('border-primary');
  });

  it('should render with small size', () => {
    const { container } = render(<LoadingSpinner size="sm" />);

    const spinner = container.firstChild as HTMLElement;
    expect(spinner).toHaveClass('h-4');
    expect(spinner).toHaveClass('w-4');
  });

  it('should render with large size', () => {
    const { container } = render(<LoadingSpinner size="lg" />);

    const spinner = container.firstChild as HTMLElement;
    expect(spinner).toHaveClass('h-16');
    expect(spinner).toHaveClass('w-16');
  });

  it('should apply custom className', () => {
    const { container } = render(<LoadingSpinner className="custom-spinner" />);

    expect(container.firstChild).toHaveClass('custom-spinner');
  });
});

describe('LoadingState', () => {
  it('should render default message', () => {
    render(<LoadingState />);

    expect(screen.getByText('Carregando...')).toBeInTheDocument();
  });

  it('should render custom message', () => {
    render(<LoadingState message="Aguarde..." />);

    expect(screen.getByText('Aguarde...')).toBeInTheDocument();
  });

  it('should render loading spinner', () => {
    const { container } = render(<LoadingState />);

    const spinner = container.querySelector('.animate-spin');
    expect(spinner).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const { container } = render(<LoadingState className="custom-loading" />);

    expect(container.firstChild).toHaveClass('custom-loading');
  });

  it('should have default full screen classes', () => {
    const { container } = render(<LoadingState />);

    const loadingState = container.firstChild as HTMLElement;
    expect(loadingState).toHaveClass('min-h-screen');
    expect(loadingState).toHaveClass('flex');
    expect(loadingState).toHaveClass('items-center');
    expect(loadingState).toHaveClass('justify-center');
  });
});