import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import userEvent from '@testing-library/user-event';
import { BarbershopList } from '@/pages/Barbershops/List';
import { useBarbershops, useDebounce } from '@/hooks';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop } from '@/types';

// Mock dependencies
const mockToast = vi.fn();
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: mockToast,
  }),
}));

vi.mock('@/hooks', () => ({
  useBarbershops: vi.fn(),
  useDebounce: vi.fn(),
}));

vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    deactivate: vi.fn(),
    reactivate: vi.fn(),
  },
}));

const navigateMock = vi.fn();
vi.mock('react-router-dom', () => ({
  useNavigate: () => navigateMock,
}));

// Mock navigator.clipboard
delete (navigator as any).clipboard;

const mockBarbershops: Barbershop[] = [
  {
    id: '1',
    name: 'Barbearia Test 1',
    document: '123.456.789-00',
    phone: '(11) 99999-9999',
    ownerName: 'João Silva',
    email: 'joao@test.com',
    code: 'ABC001',
    isActive: true,
    address: {
      zipCode: '12345-678',
      street: 'Rua Teste',
      number: '123',
      complement: 'Sala 1',
      neighborhood: 'Centro',
      city: 'São Paulo',
      state: 'SP',
    },
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    name: 'Barbearia Test 2',
    document: '987.654.321-00',
    phone: '(11) 88888-8888',
    ownerName: 'Maria Santos',
    email: 'maria@test.com',
    code: 'ABC002',
    isActive: false,
    address: {
      zipCode: '54321-876',
      street: 'Av. Principal',
      number: '456',
      complement: '',
      neighborhood: 'Jardim',
      city: 'Rio de Janeiro',
      state: 'RJ',
    },
    createdAt: '2024-01-02T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
  },
];

const mockData = {
  items: mockBarbershops,
  pageNumber: 1,
  totalPages: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  totalCount: 2,
  pageSize: 20,
};

describe('BarbershopList', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useDebounce).mockReturnValue('');
    vi.mocked(useBarbershops).mockReturnValue({
      data: mockData,
      loading: false,
      error: null,
      refetch: vi.fn(),
    });
  });

  it('renders loading state initially', () => {
    vi.mocked(useBarbershops).mockReturnValue({
      data: null,
      loading: true,
      error: null,
      refetch: vi.fn(),
    });

    render(<BarbershopList />);

    // Check for skeleton loading elements (divs with animate-pulse class)
    const skeletonElements = document.querySelectorAll('.animate-pulse');
    expect(skeletonElements.length).toBeGreaterThan(0); // Should have skeleton components
  });

  it('renders error state when there is an error', () => {
    vi.mocked(useBarbershops).mockReturnValue({
      data: null,
      loading: false,
      error: new Error('API Error'),
      refetch: vi.fn(),
    });

    render(<BarbershopList />);

    expect(screen.getByText('Erro ao carregar dados')).toBeInTheDocument();
    expect(screen.getByText('Não foi possível carregar a lista de barbearias. Tente novamente mais tarde.')).toBeInTheDocument();
    expect(screen.getByText('Tentar Novamente')).toBeInTheDocument();
  });

  it('renders empty state when no barbershops found', () => {
    vi.mocked(useBarbershops).mockReturnValue({
      data: { ...mockData, items: [] },
      loading: false,
      error: null,
      refetch: vi.fn(),
    });

    render(<BarbershopList />);

    expect(screen.getByText('Nenhuma barbearia encontrada')).toBeInTheDocument();
    expect(screen.getByText('Comece cadastrando a primeira barbearia do sistema.')).toBeInTheDocument();
    expect(screen.getByText('+ Nova Barbearia')).toBeInTheDocument();
  });

  it('renders barbershop list with data', () => {
    render(<BarbershopList />);

    expect(screen.getByText('Gestão de Barbearias')).toBeInTheDocument();
    expect(screen.getByText('Barbearia Test 1')).toBeInTheDocument();
    expect(screen.getByText('Barbearia Test 2')).toBeInTheDocument();
    expect(screen.getByText('São Paulo - SP')).toBeInTheDocument();
    expect(screen.getByText('Rio de Janeiro - RJ')).toBeInTheDocument();
  });

  it('navigates to create page when create button is clicked', async () => {
    const user = userEvent.setup();
    render(<BarbershopList />);

    const createButton = screen.getByText('+ Nova Barbearia');
    await user.click(createButton);

    expect(navigateMock).toHaveBeenCalledWith('/barbearias/nova');
  });

  it('navigates to create page from empty state', async () => {
    vi.mocked(useBarbershops).mockReturnValue({
      data: { ...mockData, items: [] },
      loading: false,
      error: null,
      refetch: vi.fn(),
    });

    const user = userEvent.setup();
    render(<BarbershopList />);

    const createButton = screen.getByText('+ Nova Barbearia');
    await user.click(createButton);

    expect(navigateMock).toHaveBeenCalledWith('/barbearias/nova');
  });

  it('filters barbershops by search term', async () => {
    const user = userEvent.setup();
    render(<BarbershopList />);

    const searchInput = screen.getByPlaceholderText('Buscar por nome, email ou cidade...');
    await user.type(searchInput, 'João');

    // useDebounce should be called with the search term
    expect(vi.mocked(useDebounce)).toHaveBeenCalledWith('João', 300);
  });

  it('filters barbershops by status', async () => {
    const user = userEvent.setup();
    render(<BarbershopList />);

    const statusSelect = screen.getByRole('combobox');

    // Select "Ativos" option by typing and selecting
    await user.click(statusSelect);
    await user.keyboard('Ativos');
    await user.keyboard('{Enter}');

    // useBarbershops should be called with active filter
    expect(vi.mocked(useBarbershops)).toHaveBeenCalledWith(
      expect.objectContaining({
        isActive: true,
      })
    );
  });

  it('navigates to barbershop details when view button is clicked', async () => {
    const user = userEvent.setup();
    render(<BarbershopList />);

    // Find and click the view button for the first barbershop
    const viewButtons = screen.getAllByText('Ver');
    await user.click(viewButtons[0]);

    expect(navigateMock).toHaveBeenCalledWith('/barbearias/1');
  });

  it('navigates to edit page when edit button is clicked', async () => {
    const user = userEvent.setup();
    render(<BarbershopList />);

    // Find and click the edit button for the first barbershop
    const editButtons = screen.getAllByText('Editar');
    await user.click(editButtons[0]);

    expect(navigateMock).toHaveBeenCalledWith('/barbearias/1/editar');
  });

  it('reactivates barbershop successfully', async () => {
    vi.mocked(barbershopService.reactivate).mockResolvedValue(undefined);
    const refetchMock = vi.fn();

    vi.mocked(useBarbershops).mockReturnValue({
      data: mockData,
      loading: false,
      error: null,
      refetch: refetchMock,
    });

    const user = userEvent.setup();
    render(<BarbershopList />);

    // Find and click the reactivate button for the inactive barbershop
    const reactivateButtons = screen.getAllByText('Reativar');
    await user.click(reactivateButtons[0]);

    await waitFor(() => {
      expect(barbershopService.reactivate).toHaveBeenCalledWith('2');
    });

    expect(mockToast).toHaveBeenCalledWith({
      title: 'Barbearia reativada com sucesso!',
      description: 'Barbearia Test 2 foi reativada.',
    });

    expect(refetchMock).toHaveBeenCalled();
  });

  it('handles reactivation error', async () => {
    vi.mocked(barbershopService.reactivate).mockRejectedValue(new Error('API Error'));

    const user = userEvent.setup();
    render(<BarbershopList />);

    // Find and click the reactivate button for the inactive barbershop
    const reactivateButtons = screen.getAllByText('Reativar');
    await user.click(reactivateButtons[0]);

    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Erro ao reativar barbearia',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    });
  });

  it.skip('copies barbershop code to clipboard', async () => {
    const user = userEvent.setup();
    render(<BarbershopList />);

    // Find and click the copy button (which shows the code ABC001)
    const copyButton = screen.getByText('ABC001');
    await user.click(copyButton);

    // Note: Clipboard API is not fully supported in jsdom
    // In a real browser environment, this would copy to clipboard
    expect(mockToast).toHaveBeenCalledWith({
      title: 'Código copiado!',
      description: 'O código ABC001 foi copiado para a área de transferência.',
    });
  });

  it('deactivates barbershop successfully through modal', async () => {
    vi.mocked(barbershopService.deactivate).mockResolvedValue(undefined);
    const refetchMock = vi.fn();

    vi.mocked(useBarbershops).mockReturnValue({
      data: mockData,
      loading: false,
      error: null,
      refetch: refetchMock,
    });

    const user = userEvent.setup();
    render(<BarbershopList />);

    // Open deactivate modal
    const deactivateButtons = screen.getAllByText('Desativar');
    await user.click(deactivateButtons[0]);

    // Confirm deactivation
    const confirmButton = screen.getByRole('button', { name: 'Confirmar desativação da barbearia' });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(barbershopService.deactivate).toHaveBeenCalledWith('1');
    });

    expect(mockToast).toHaveBeenCalledWith({
      title: 'Barbearia desativada com sucesso!',
      description: 'Barbearia Test 1 foi desativada.',
    });

    expect(refetchMock).toHaveBeenCalled();
  });

  it('handles deactivation error through modal', async () => {
    vi.mocked(barbershopService.deactivate).mockRejectedValue(new Error('API Error'));

    const user = userEvent.setup();
    render(<BarbershopList />);

    // Open deactivate modal
    const deactivateButtons = screen.getAllByText('Desativar');
    await user.click(deactivateButtons[0]);

    // Confirm deactivation
    const confirmButton = screen.getByRole('button', { name: 'Confirmar desativação da barbearia' });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Erro ao desativar barbearia',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    });
  });

  it('closes deactivate modal when cancel is clicked', async () => {
    const user = userEvent.setup();
    render(<BarbershopList />);

    // Open deactivate modal
    const deactivateButtons = screen.getAllByText('Desativar');
    await user.click(deactivateButtons[0]);

    expect(screen.getByRole('heading', { name: 'Confirmar Desativação' })).toBeInTheDocument();

    // Cancel deactivation
    const cancelButton = screen.getByRole('button', { name: 'Cancelar desativação' });
    await user.click(cancelButton);

    expect(screen.queryByRole('heading', { name: 'Confirmar Desativação' })).not.toBeInTheDocument();
  });

  it('renders pagination when there are multiple pages', () => {
    const paginatedData = {
      ...mockData,
      pageNumber: 1,
      totalPages: 3,
      hasNextPage: true,
    };

    vi.mocked(useBarbershops).mockReturnValue({
      data: paginatedData,
      loading: false,
      error: null,
      refetch: vi.fn(),
    });

    render(<BarbershopList />);

    // Pagination component should be rendered
    expect(screen.getByText('Página 1 de 3')).toBeInTheDocument();
  });
});