import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LoginAdminBarbearia } from '../LoginAdminBarbearia';
import { vi, describe, it, expect, beforeEach } from 'vitest';

// Mock the hooks
vi.mock('@/hooks/useBarbeariaCode', () => ({
  useBarbeariaCode: vi.fn(),
}));

vi.mock('@/contexts/BarbeariaContext', () => ({
  useBarbearia: vi.fn(),
}));

vi.mock('@/services/adminBarbeariaAuth.service', () => ({
  adminBarbeariaAuthService: {
    login: vi.fn(),
  },
}));

vi.mock('@/hooks/use-toast', () => ({
  useToast: vi.fn(() => ({
    toast: vi.fn(),
    dismiss: vi.fn(),
    toasts: [],
  })),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: vi.fn(() => vi.fn()),
  };
});

// Import mocked modules
import { useBarbeariaCode } from '@/hooks/useBarbeariaCode';
import { useBarbearia } from '@/contexts/BarbeariaContext';
import { adminBarbeariaAuthService } from '@/services/adminBarbeariaAuth.service';
import { useToast } from '@/hooks/use-toast';
import { useNavigate } from 'react-router-dom';

const mockUseBarbeariaCode = vi.mocked(useBarbeariaCode);
const mockUseBarbearia = vi.mocked(useBarbearia);
const mockAdminBarbeariaAuthService = vi.mocked(adminBarbeariaAuthService);
const mockUseToast = vi.mocked(useToast);
const mockUseNavigate = vi.mocked(useNavigate);

describe('LoginAdminBarbearia', () => {
  const mockToast = vi.fn();
  const mockNavigate = vi.fn();
  const mockSetBarbearia = vi.fn();
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
        mutations: {
          retry: false,
        },
      },
    });

    mockUseToast.mockReturnValue({
      toast: mockToast,
      dismiss: vi.fn(),
      toasts: []
    });
    mockUseNavigate.mockReturnValue(mockNavigate);
    mockUseBarbearia.mockReturnValue({
      barbearia: null,
      setBarbearia: mockSetBarbearia,
      clearBarbearia: vi.fn(),
      isLoaded: true,
    });
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  it('should display loading state while validating code', () => {
    mockUseBarbeariaCode.mockReturnValue({
      codigo: '6SJJRFPD',
      barbeariaInfo: null,
      isLoading: true,
      error: null,
      isValidating: true,
    });

    render(<LoginAdminBarbearia />, { wrapper });

    expect(screen.getByText('Validando código da barbearia...')).toBeInTheDocument();
  });

  it('should display error for invalid barbershop code (404)', () => {
    const error = new Error('Request failed with status 404');
    mockUseBarbeariaCode.mockReturnValue({
      codigo: 'INVALID',
      barbeariaInfo: null,
      isLoading: false,
      error,
      isValidating: false,
    });

    render(<LoginAdminBarbearia />, { wrapper });

    expect(screen.getByText('Barbearia não encontrada')).toBeInTheDocument();
    expect(screen.getByText('O código da barbearia informado não existe ou é inválido.')).toBeInTheDocument();
  });

  it('should display login form when code is valid', () => {
    mockUseBarbeariaCode.mockReturnValue({
      codigo: '6SJJRFPD',
      barbeariaInfo: {
        id: '123',
        nome: 'Barbearia Test',
        codigo: '6SJJRFPD',
        isActive: true,
      },
      isLoading: false,
      error: null,
      isValidating: false,
    });

    render(<LoginAdminBarbearia />, { wrapper });

    expect(screen.getByText('Barbearia Test')).toBeInTheDocument();
    expect(screen.getByLabelText('Email')).toBeInTheDocument();
    expect(screen.getByLabelText('Senha')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Entrar' })).toBeInTheDocument();
  });

  it('should submit form and call login service', async () => {
    const user = userEvent.setup();

    mockUseBarbeariaCode.mockReturnValue({
      codigo: '6SJJRFPD',
      barbeariaInfo: {
        id: '123',
        nome: 'Barbearia Test',
        codigo: '6SJJRFPD',
        isActive: true,
      },
      isLoading: false,
      error: null,
      isValidating: false,
    });

    mockAdminBarbeariaAuthService.login.mockResolvedValue({
      token: 'fake-token',
      barbeariaId: '123',
      nome: 'Barbearia Test',
      codigo: '6SJJRFPD',
      expiresAt: '2024-12-31T23:59:59Z',
    });

    render(<LoginAdminBarbearia />, { wrapper });

    await user.type(screen.getByLabelText('Email'), 'admin@test.com');
    await user.type(screen.getByLabelText('Senha'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(mockAdminBarbeariaAuthService.login).toHaveBeenCalledWith({
        codigo: '6SJJRFPD',
        email: 'admin@test.com',
        senha: 'password123',
      });
    });
  });

  it('should show success toast and navigate on successful login', async () => {
    const user = userEvent.setup();

    mockUseBarbeariaCode.mockReturnValue({
      codigo: '6SJJRFPD',
      barbeariaInfo: {
        id: '123',
        nome: 'Barbearia Test',
        codigo: '6SJJRFPD',
        isActive: true,
      },
      isLoading: false,
      error: null,
      isValidating: false,
    });

    mockAdminBarbeariaAuthService.login.mockResolvedValue({
      token: 'fake-token',
      barbeariaId: '123',
      nome: 'Barbearia Test',
      codigo: '6SJJRFPD',
      expiresAt: '2024-12-31T23:59:59Z',
    });

    render(<LoginAdminBarbearia />, { wrapper });

    await user.type(screen.getByLabelText('Email'), 'admin@test.com');
    await user.type(screen.getByLabelText('Senha'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Login realizado com sucesso!',
        description: 'Bem-vindo à Barbearia Test!',
      });
      expect(mockSetBarbearia).toHaveBeenCalledWith({
        barbeariaId: '123',
        nome: 'Barbearia Test',
        codigo: '6SJJRFPD',
        isActive: true,
      });
      expect(mockNavigate).toHaveBeenCalledWith('/6SJJRFPD/dashboard');
    });
  });

  it('should show error toast on login failure (401)', async () => {
    const user = userEvent.setup();

    mockUseBarbeariaCode.mockReturnValue({
      codigo: '6SJJRFPD',
      barbeariaInfo: {
        id: '123',
        nome: 'Barbearia Test',
        codigo: '6SJJRFPD',
        isActive: true,
      },
      isLoading: false,
      error: null,
      isValidating: false,
    });

    const error = new Error('Unauthorized') as any;
    error.response = { status: 401 };
    mockAdminBarbeariaAuthService.login.mockRejectedValue(error);

    render(<LoginAdminBarbearia />, { wrapper });

    await user.type(screen.getByLabelText('Email'), 'admin@test.com');
    await user.type(screen.getByLabelText('Senha'), 'wrongpassword');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Credenciais inválidas',
        description: 'Email ou senha incorretos. Verifique e tente novamente.',
        variant: 'destructive',
      });
    });
  });
});