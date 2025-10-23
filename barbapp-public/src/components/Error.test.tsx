import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ErrorState } from './Error';

describe('ErrorState', () => {
  it('should render default title and message', () => {
    render(<ErrorState />);

    expect(screen.getByText('Erro')).toBeInTheDocument();
    expect(screen.getByText('Ocorreu um erro inesperado.')).toBeInTheDocument();
  });

  it('should render custom title and message', () => {
    render(
      <ErrorState
        title="Erro de Conexão"
        message="Não foi possível conectar ao servidor."
      />
    );

    expect(screen.getByText('Erro de Conexão')).toBeInTheDocument();
    expect(screen.getByText('Não foi possível conectar ao servidor.')).toBeInTheDocument();
  });

  it('should render retry button when onRetry is provided', () => {
    const mockOnRetry = vi.fn();
    render(<ErrorState onRetry={mockOnRetry} />);

    const retryButton = screen.getByText('Tentar Novamente');
    expect(retryButton).toBeInTheDocument();
  });

  it('should not render retry button when onRetry is not provided', () => {
    render(<ErrorState />);

    expect(screen.queryByText('Tentar Novamente')).not.toBeInTheDocument();
  });

  it('should call onRetry when retry button is clicked', () => {
    const mockOnRetry = vi.fn();
    render(<ErrorState onRetry={mockOnRetry} />);

    const retryButton = screen.getByText('Tentar Novamente');
    fireEvent.click(retryButton);

    expect(mockOnRetry).toHaveBeenCalledTimes(1);
  });

  it('should apply custom className', () => {
    const { container } = render(<ErrorState className="custom-error" />);

    expect(container.firstChild).toHaveClass('custom-error');
  });

  it('should have default full screen classes', () => {
    const { container } = render(<ErrorState />);

    const errorState = container.firstChild as HTMLElement;
    expect(errorState).toHaveClass('min-h-screen');
    expect(errorState).toHaveClass('flex');
    expect(errorState).toHaveClass('items-center');
    expect(errorState).toHaveClass('justify-center');
  });
});