import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ProtectedRoute } from '@/components/ProtectedRoute';

// Mock react-router-dom
vi.mock('react-router-dom', () => ({
  Navigate: ({ to }: { to: string }) => <div data-testid="navigate" data-to={to} />,
  Outlet: () => <div data-testid="outlet">Outlet Content</div>,
}));

// Mock useAuth hook
vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}));

// Mock Header component
vi.mock('@/components/layout/Header', () => ({
  Header: () => <div data-testid="header">Header</div>,
}));

import { useAuth } from '@/hooks/useAuth';

describe('ProtectedRoute', () => {
  const mockUseAuth = vi.mocked(useAuth);

  it('should render Navigate to login when not authenticated', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: false,
      logout: vi.fn(),
    });

    render(<ProtectedRoute />);

    const navigate = screen.getByTestId('navigate');
    expect(navigate).toBeInTheDocument();
    expect(navigate).toHaveAttribute('data-to', '/admin/login');
  });

  it('should render protected content when authenticated', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: vi.fn(),
    });

    render(<ProtectedRoute />);

    expect(screen.getByTestId('header')).toBeInTheDocument();
    expect(screen.getByTestId('outlet')).toBeInTheDocument();
    expect(screen.queryByTestId('navigate')).not.toBeInTheDocument();
  });

  it('should render with correct layout structure when authenticated', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: vi.fn(),
    });

    const { container } = render(<ProtectedRoute />);

    // Check for main layout classes
    const mainDiv = container.firstChild as HTMLElement;
    expect(mainDiv).toHaveClass('min-h-screen', 'bg-gray-50');

    // Check for header
    expect(screen.getByTestId('header')).toBeInTheDocument();

    // Check for main content area
    const main = screen.getByRole('main');
    expect(main).toHaveClass('container', 'mx-auto', 'px-4', 'py-8');
  });
});