import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { Header } from '@/components/layout/Header';
import { useAuth } from '@/hooks/useAuth';

// Mock the useAuth hook
vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
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