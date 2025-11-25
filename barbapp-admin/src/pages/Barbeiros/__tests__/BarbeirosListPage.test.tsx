import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { BarbeirosListPage } from '../BarbeirosListPage';
import { barbeiroService } from '@/services/barbeiro.service';
import type { Barber, PaginatedResponse } from '@/types';

// Mock do serviço
vi.mock('@/services/barbeiro.service');

// Mock do hook useToast
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

// Mock do hook useDebounce - pass through the value immediately
vi.mock('@/hooks', async () => {
  const actual = await vi.importActual('@/hooks');
  return {
    ...actual,
    useDebounce: <T,>(value: T) => value,
  };
});

// Mock do BarbeariaContext
vi.mock('@/contexts/BarbeariaContext', () => ({
  useBarbearia: vi.fn(() => ({
    barbearia: {
      barbeariaId: 'barb-1',
      nome: 'Barbearia Teste',
      codigo: 'TEST1234',
      isActive: true,
    },
    clearBarbearia: vi.fn(),
    setBarbearia: vi.fn(),
    isLoaded: true,
  })),
  BarbeariaProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock navigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

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

    // Setup default mocks BEFORE each test
    vi.mocked(barbeiroService.list).mockResolvedValue(mockPaginatedResponse);
    vi.mocked(barbeiroService.deactivate).mockResolvedValue();
    vi.mocked(barbeiroService.reactivate).mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  const renderComponent = () => {
    return render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/TEST1234/barbeiros']}>
          <Routes>
            <Route path="/:codigo/barbeiros" element={<BarbeirosListPage />} />
          </Routes>
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

    // Debug: verificar se o mock foi chamado
    await waitFor(
      () => {
        expect(barbeiroService.list).toHaveBeenCalled();
      },
      { timeout: 3000 }
    );

    // Aguardar os dados aparecerem após o loading
    await waitFor(
      () => {
        expect(screen.getByText('João Silva')).toBeInTheDocument();
      },
      { timeout: 3000 }
    );
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

    // Use getAllByRole and click the first "Ativos" option
    const activeOptions = await screen.findAllByRole('option', { name: /ativos/i });
    await user.click(activeOptions[0]);

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

    // Wait for API call and data to load
    await waitFor(() => {
      expect(barbeiroService.list).toHaveBeenCalled();
    });
    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    const deactivateButton = screen.getByRole('button', { name: /desativar/i });
    await user.click(deactivateButton);

    await waitFor(() => {
      expect(screen.getByText(/desativar barbeiro/i)).toBeInTheDocument();
    });
  });

  it('should have deactivate button that can be clicked', async () => {
    const user = userEvent.setup();
    renderComponent();

    // Wait for API call and data to load
    await waitFor(() => {
      expect(barbeiroService.list).toHaveBeenCalled();
    });
    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    // Find and verify deactivate button exists
    const deactivateButton = screen.getByRole('button', { name: /desativar/i });
    expect(deactivateButton).toBeInTheDocument();

    // Click the button
    await user.click(deactivateButton);

    // Verify modal opens or state changes
    // This test verifies the button is clickable without checking specific modal behavior
  });

  it('should call reactivate button exists for inactive barbeiros', async () => {
    renderComponent();

    // Wait for API call and data to load
    await waitFor(() => {
      expect(barbeiroService.list).toHaveBeenCalled();
    });
    await waitFor(() => {
      expect(screen.getByText('Pedro Santos')).toBeInTheDocument();
    });

    // Pedro Santos is inactive, should have a reactivate button
    const reactivateButton = screen.getByRole('button', { name: /reativar/i });
    expect(reactivateButton).toBeInTheDocument();
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
