import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { Header } from '../Header';
import * as adminBarbeariaAuthService from '@/services/adminBarbeariaAuth.service';
import * as BarbeariaContextModule from '@/contexts/BarbeariaContext';

// Mock the auth service
vi.mock('@/services/adminBarbeariaAuth.service', () => ({
  adminBarbeariaAuthService: {
    logout: vi.fn(),
  },
}));

// Mock the BarbeariaContext
const mockClearBarbearia = vi.fn();
vi.mock('@/contexts/BarbeariaContext', async () => {
  const actual = await vi.importActual('@/contexts/BarbeariaContext');
  return {
    ...actual,
    useBarbearia: vi.fn(() => ({
      barbearia: {
        barbeariaId: 'test-id',
        nome: 'Barbearia Teste',
        codigo: 'TEST1234',
        isActive: true,
      },
      clearBarbearia: mockClearBarbearia,
      setBarbearia: vi.fn(),
      isLoaded: true,
    })),
  };
});

// Mock navigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

// Helper to render with providers
const renderWithProviders = (ui: React.ReactElement) => {
  return render(
    <MemoryRouter initialEntries={['/TEST1234']}>
      <Routes>
        <Route path="/:codigo/*" element={ui} />
      </Routes>
    </MemoryRouter>
  );
};

describe('Header', () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should render barbershop name and code', () => {
    renderWithProviders(<Header />);

    expect(screen.getByText('Barbearia Teste')).toBeInTheDocument();
    expect(screen.getByText(/Código: TEST1234/i)).toBeInTheDocument();
  });

  it('should render logout button', () => {
    renderWithProviders(<Header />);

    const logoutButton = screen.getByRole('button', { name: /sair/i });
    expect(logoutButton).toBeInTheDocument();
  });

  it('should call logout and navigate on logout button click', async () => {
    const user = userEvent.setup();
    renderWithProviders(<Header />);

    const logoutButton = screen.getByRole('button', { name: /sair/i });
    await user.click(logoutButton);

    expect(adminBarbeariaAuthService.adminBarbeariaAuthService.logout).toHaveBeenCalledTimes(1);
    expect(mockClearBarbearia).toHaveBeenCalledTimes(1);
    expect(mockNavigate).toHaveBeenCalledWith('/TEST1234/login');
  });

  it('should render menu toggle button when onMenuToggle is provided', () => {
    const onMenuToggle = vi.fn();
    renderWithProviders(<Header onMenuToggle={onMenuToggle} />);

    const menuButton = screen.getByLabelText(/toggle menu/i);
    expect(menuButton).toBeInTheDocument();
  });

  it('should call onMenuToggle when menu button is clicked', async () => {
    const user = userEvent.setup();
    const onMenuToggle = vi.fn();
    renderWithProviders(<Header onMenuToggle={onMenuToggle} />);

    const menuButton = screen.getByLabelText(/toggle menu/i);
    await user.click(menuButton);

    expect(onMenuToggle).toHaveBeenCalledTimes(1);
  });
});
