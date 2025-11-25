import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { Header } from '@/components/layout/Header';
import { useAuth } from '@/hooks/useAuth';

// Mock the useAuth hook from hooks
vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}));

// Mock the useAuth (AuthContext) - used for barbeiro auth check
vi.mock('@/contexts/AuthContext', () => ({
  useAuth: vi.fn(() => ({
    user: null,
    isAuthenticated: false,
    isLoading: false,
    login: vi.fn(),
    logout: vi.fn(),
  })),
}));

// Mock BarbershopSelector to avoid unnecessary dependencies
vi.mock('./BarbershopSelector', () => ({
  BarbershopSelector: () => null,
}));

const mockUseAuth = vi.mocked(useAuth);

describe('Header', () => {
  it('should render the app name', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: vi.fn(),
    });

    render(<Header />);

    expect(screen.getByText('BarbApp Admin')).toBeInTheDocument();
  });

  it('should render the logout button', () => {
    const mockLogout = vi.fn();
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: mockLogout,
    });

    render(<Header />);

    const logoutButton = screen.getByRole('button', { name: /sair/i });
    expect(logoutButton).toBeInTheDocument();
  });

  it('should call logout when logout button is clicked', async () => {
    const user = userEvent.setup();
    const mockLogout = vi.fn();
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: mockLogout,
    });

    render(<Header />);

    const logoutButton = screen.getByRole('button', { name: /sair/i });
    await user.click(logoutButton);

    expect(mockLogout).toHaveBeenCalledTimes(1);
  });

  it('should have proper accessibility attributes', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: vi.fn(),
    });

    render(<Header />);

    const logoutButton = screen.getByRole('button', { name: /sair/i });
    expect(logoutButton).toHaveAttribute('type', 'button');
  });
});