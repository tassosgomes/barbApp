import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { BarbeirosListPage } from '../BarbeirosListPage';
import { barbeiroService } from '@/services/barbeiro.service';
import { BarbeariaProvider } from '@/contexts/BarbeariaContext';
import type { Barber, PaginatedResponse } from '@/types';

// Mock do serviço
vi.mock('@/services/barbeiro.service');

// Mock do hook useToast
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

// Mock do hook useDebounce
vi.mock('@/hooks', () => ({
  useDebounce: (value: string) => value,
}));

const mockBarbeiros: Barber[] = [
  {
    id: '1',
    name: 'João Silva',
    email: 'joao@example.com',
    phoneFormatted: '(11) 98765-4321',
    services: [
      { id: 's1', name: 'Corte' },
      { id: 's2', name: 'Barba' },
    ],
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    name: 'Pedro Santos',
    email: 'pedro@example.com',
    phoneFormatted: '(11) 91234-5678',
    services: [{ id: 's1', name: 'Corte' }],
    isActive: false,
    createdAt: '2024-01-02T00:00:00Z',
  },
];

const mockPaginatedResponse: PaginatedResponse<Barber> = {
  items: mockBarbeiros,
  pageNumber: 1,
  pageSize: 20,
  totalPages: 1,
  totalCount: 2,
  hasPreviousPage: false,
  hasNextPage: false,
};

describe('BarbeirosListPage', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });

    // Setup mock context data with correct format (matching BarbeariaContext schema)
    localStorage.setItem(
      'admin_barbearia_context',
      JSON.stringify({
        id: 'barb-1',
        nome: 'Barbearia Teste',
        codigo: 'TEST1234',
        isActive: true,
      })
    );

    // Setup default mocks BEFORE each test
    vi.mocked(barbeiroService.list).mockResolvedValue(mockPaginatedResponse);
    vi.mocked(barbeiroService.deactivate).mockResolvedValue();
    vi.mocked(barbeiroService.reactivate).mockResolvedValue();
  });

  afterEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  const renderComponent = () => {
    return render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/TEST1234/barbeiros']}>
          <BarbeariaProvider>
            <Routes>
              <Route path="/:codigo/barbeiros" element={<BarbeirosListPage />} />
            </Routes>
          </BarbeariaProvider>
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('should render page title and description', () => {
    renderComponent();

    expect(screen.getByText('Barbeiros')).toBeInTheDocument();
    expect(screen.getByText('Gerencie sua equipe de barbeiros.')).toBeInTheDocument();
  });

  it('should render "Novo Barbeiro" button', () => {
    renderComponent();

    expect(screen.getByRole('button', { name: /novo barbeiro/i })).toBeInTheDocument();
  });

  it('should display barbeiros in table', async () => {
    renderComponent();

    // Aguardar o loading state desaparecer e os dados aparecerem
    await waitFor(
      () => {
        expect(screen.queryByText(/nenhum barbeiro encontrado/i)).not.toBeInTheDocument();
      },
      { timeout: 3000 }
    );

    expect(screen.getByText('João Silva')).toBeInTheDocument();
    expect(screen.getByText('joao@example.com')).toBeInTheDocument();
    expect(screen.getByText('Pedro Santos')).toBeInTheDocument();
  });

  it('should display active/inactive status badges', async () => {
    renderComponent();

    await waitFor(() => {
      const activeBadges = screen.getAllByText('Ativo');
      const inactiveBadges = screen.getAllByText('Inativo');
      expect(activeBadges).toHaveLength(1);
      expect(inactiveBadges).toHaveLength(1);
    });
  });

  it('should display services for each barbeiro', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getAllByText('Corte')).toHaveLength(2);
      expect(screen.getByText('Barba')).toBeInTheDocument();
    });
  });

  it('should have edit buttons for each barbeiro', async () => {
    renderComponent();

    await waitFor(() => {
      const editButtons = screen.getAllByRole('button', { name: /editar/i });
      expect(editButtons).toHaveLength(2);
    });
  });

  it('should have deactivate button for active barbeiros', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /desativar/i })).toBeInTheDocument();
    });
  });

  it('should have reactivate button for inactive barbeiros', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /reativar/i })).toBeInTheDocument();
    });
  });

  it('should filter barbeiros by search term', async () => {
    const user = userEvent.setup();
    renderComponent();

    const searchInput = screen.getByPlaceholderText(/buscar por nome ou email/i);

    await user.type(searchInput, 'João');

    await waitFor(() => {
      expect(barbeiroService.list).toHaveBeenCalledWith(
        expect.objectContaining({
          search: 'João',
        })
      );
    });
  });

  it('should filter barbeiros by status', async () => {
    const user = userEvent.setup();
    renderComponent();

    const statusFilter = screen.getByRole('combobox');
    await user.click(statusFilter);

    const activeOption = await screen.findByRole('option', { name: /ativos/i });
    await user.click(activeOption);

    await waitFor(() => {
      expect(barbeiroService.list).toHaveBeenCalledWith(
        expect.objectContaining({
          isActive: true,
        })
      );
    });
  });

  it('should open deactivate confirmation modal when clicking desativar', async () => {
    const user = userEvent.setup();
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    const deactivateButton = screen.getByRole('button', { name: /desativar/i });
    await user.click(deactivateButton);

    expect(screen.getByText(/desativar barbeiro/i)).toBeInTheDocument();
    expect(screen.getByText(/joão silva/i)).toBeInTheDocument();
  });

  it('should call deactivate service when confirming in modal', async () => {
    const user = userEvent.setup();
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    const deactivateButton = screen.getByRole('button', { name: /desativar/i });
    await user.click(deactivateButton);

    const confirmButton = within(screen.getByRole('dialog')).getByRole('button', {
      name: /desativar/i,
    });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(barbeiroService.deactivate).toHaveBeenCalledWith('1');
    });
  });

  it('should call reactivate service when clicking reativar', async () => {
    const user = userEvent.setup();
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Pedro Santos')).toBeInTheDocument();
    });

    const reactivateButton = screen.getByRole('button', { name: /reativar/i });
    await user.click(reactivateButton);

    await waitFor(() => {
      expect(barbeiroService.reactivate).toHaveBeenCalledWith('2');
    });
  });

  it('should display empty state when no barbeiros found', async () => {
    vi.mocked(barbeiroService.list).mockResolvedValue({
      ...mockPaginatedResponse,
      items: [],
      totalCount: 0,
    });

    renderComponent();

    await waitFor(() => {
      expect(
        screen.getByText(/nenhum barbeiro encontrado/i)
      ).toBeInTheDocument();
    });
  });

  it('should display loading state while fetching data', () => {
    vi.mocked(barbeiroService.list).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    renderComponent();

    // Skeleton loader should be visible
    expect(screen.getByRole('table')).toBeInTheDocument();
  });
});
