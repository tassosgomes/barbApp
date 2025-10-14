import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import userEvent from '@testing-library/user-event';
import { BarbershopDetails } from '@/pages/Barbershops/Details';
import { barbershopService } from '@/services/barbershop.service';
import { copyToClipboard } from '@/lib/utils';
import type { Barbershop } from '@/types';

// Mock dependencies
const mockToast = vi.fn();
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: mockToast,
  }),
}));

vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    getById: vi.fn(),
    deactivate: vi.fn(),
    reactivate: vi.fn(),
  },
}));

vi.mock('@/lib/utils', () => ({
  copyToClipboard: vi.fn(),
  cn: vi.fn((...inputs: any[]) => inputs.join(' ')), // Simple mock for className utility
}));

const navigateMock = vi.fn();
vi.mock('react-router-dom', () => ({
  useParams: () => ({ id: '1' }),
  useNavigate: () => navigateMock,
}));

const mockBarbershop: Barbershop = {
  id: '1',
  name: 'Test Barbershop',
  document: '123.456.789-00',
  phone: '(11) 99999-9999',
  ownerName: 'John Doe',
  email: 'test@example.com',
  code: 'ABC123',
  isActive: true,
  address: {
    zipCode: '12345-678',
    street: 'Test Street',
    number: '123',
    complement: 'Apt 1',
    neighborhood: 'Test Neighborhood',
    city: 'Test City',
    state: 'SP',
  },
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
};

describe('BarbershopDetails', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders loading state initially', () => {
    vi.mocked(barbershopService.getById).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    render(<BarbershopDetails />);

    // Check for skeleton loading elements (divs with animate-pulse class)
    const skeletonElements = document.querySelectorAll('.animate-pulse');
    expect(skeletonElements.length).toBeGreaterThan(10); // Should have multiple skeleton components
  });

  it('renders barbershop details when data loads successfully', async () => {
    vi.mocked(barbershopService.getById).mockResolvedValue(mockBarbershop);

    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Barbershop' })).toBeInTheDocument();
    });

    expect(screen.getByText('ABC123')).toBeInTheDocument();
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('test@example.com')).toBeInTheDocument();
    expect(screen.getByText('(11) 99999-9999')).toBeInTheDocument();
  });

  it('navigates back when back button is clicked', async () => {
    vi.mocked(barbershopService.getById).mockResolvedValue(mockBarbershop);

    const user = userEvent.setup();
    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Barbershop' })).toBeInTheDocument();
    });

    const backButton = screen.getByText('Voltar');
    await user.click(backButton);

    expect(navigateMock).toHaveBeenCalledWith(-1);
  });

  it('navigates to edit page when edit button is clicked', async () => {
    vi.mocked(barbershopService.getById).mockResolvedValue(mockBarbershop);

    const user = userEvent.setup();
    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Barbershop' })).toBeInTheDocument();
    });

    const editButton = screen.getByText('Editar');
    await user.click(editButton);

    expect(navigateMock).toHaveBeenCalledWith('/barbearias/1/editar');
  });

  it('copies code to clipboard successfully', async () => {
    vi.mocked(barbershopService.getById).mockResolvedValue(mockBarbershop);
    vi.mocked(copyToClipboard).mockResolvedValue(true);

    const user = userEvent.setup();
    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Barbershop' })).toBeInTheDocument();
    });

    const copyButton = screen.getByText('Copiar Código');
    await user.click(copyButton);

    expect(copyToClipboard).toHaveBeenCalledWith('ABC123');
    expect(mockToast).toHaveBeenCalledWith({
      title: 'Código copiado!',
      description: 'O código da barbearia foi copiado para a área de transferência.',
    });
  });

  it('handles copy code failure', async () => {
    vi.mocked(barbershopService.getById).mockResolvedValue(mockBarbershop);
    vi.mocked(copyToClipboard).mockResolvedValue(false);

    const user = userEvent.setup();
    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Barbershop' })).toBeInTheDocument();
    });

    const copyButton = screen.getByText('Copiar Código');
    await user.click(copyButton);

    expect(mockToast).toHaveBeenCalledWith({
      title: 'Erro ao copiar',
      description: 'Não foi possível copiar o código.',
      variant: 'destructive',
    });
  });

  it('deactivates barbershop successfully', async () => {
    vi.mocked(barbershopService.getById).mockResolvedValue(mockBarbershop);
    vi.mocked(barbershopService.deactivate).mockResolvedValue(undefined);
    vi.mocked(barbershopService.getById).mockResolvedValueOnce(mockBarbershop).mockResolvedValueOnce({
      ...mockBarbershop,
      isActive: false,
    });

    const user = userEvent.setup();
    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Barbershop' })).toBeInTheDocument();
    });

    const deactivateButton = screen.getByText('Desativar');
    await user.click(deactivateButton);

    await waitFor(() => {
      expect(barbershopService.deactivate).toHaveBeenCalledWith('1');
    });

    expect(mockToast).toHaveBeenCalledWith({
      title: 'Barbearia desativada com sucesso!',
    });
  });

  it('reactivates barbershop successfully', async () => {
    const inactiveBarbershop = { ...mockBarbershop, isActive: false };
    vi.mocked(barbershopService.getById).mockResolvedValue(inactiveBarbershop);
    vi.mocked(barbershopService.reactivate).mockResolvedValue(undefined);
    vi.mocked(barbershopService.getById).mockResolvedValueOnce(inactiveBarbershop).mockResolvedValueOnce(mockBarbershop);

    const user = userEvent.setup();
    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Barbershop' })).toBeInTheDocument();
    });

    const reactivateButton = screen.getByText('Reativar');
    await user.click(reactivateButton);

    await waitFor(() => {
      expect(barbershopService.reactivate).toHaveBeenCalledWith('1');
    });

    expect(mockToast).toHaveBeenCalledWith({
      title: 'Barbearia reativada com sucesso!',
    });
  });

  it('handles deactivation error', async () => {
    vi.mocked(barbershopService.getById).mockResolvedValue(mockBarbershop);
    vi.mocked(barbershopService.deactivate).mockRejectedValue(new Error('API Error'));

    const user = userEvent.setup();
    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Barbershop' })).toBeInTheDocument();
    });

    const deactivateButton = screen.getByText('Desativar');
    await user.click(deactivateButton);

    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Erro ao alterar status',
        description: 'Não foi possível alterar o status da barbearia.',
        variant: 'destructive',
      });
    });
  });

  it('handles loading error and navigates to list', async () => {
    vi.mocked(barbershopService.getById).mockRejectedValue(new Error('Not found'));

    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(navigateMock).toHaveBeenCalledWith('/barbearias');
    });

    expect(mockToast).toHaveBeenCalledWith({
      title: 'Erro ao carregar barbearia',
      description: 'Não foi possível carregar os dados da barbearia.',
      variant: 'destructive',
    });
  });

  it('renders not found state when barbershop is null', async () => {
    vi.mocked(barbershopService.getById).mockResolvedValue(null as any);

    render(<BarbershopDetails />);

    await waitFor(() => {
      expect(screen.getByText('Barbearia não encontrada')).toBeInTheDocument();
    });

    expect(screen.getByText('Voltar à lista')).toBeInTheDocument();
  });
});