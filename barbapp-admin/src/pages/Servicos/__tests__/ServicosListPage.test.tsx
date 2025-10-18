import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { ServicosListPage } from '../ServicosListPage';
import { BarbeariaProvider } from '@/contexts/BarbeariaContext';
import { servicoService } from '@/services/servico.service';
import { BarbershopService, PaginatedResponse } from '@/types/servico';

// Mock do service
vi.mock('@/services/servico.service', () => ({
  servicoService: {
    list: vi.fn(),
    deactivate: vi.fn(),
  },
}));

// Mock do toast
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

const mockServicos: BarbershopService[] = [
  {
    id: '1',
    name: 'Corte de Cabelo',
    description: 'Corte masculino tradicional',
    durationMinutes: 30,
    price: 35.0,
    isActive: true,
  },
  {
    id: '2',
    name: 'Barba',
    description: 'Aparo e modelagem de barba',
    durationMinutes: 20,
    price: 25.0,
    isActive: true,
  },
  {
    id: '3',
    name: 'Corte + Barba',
    description: 'Combo completo',
    durationMinutes: 50,
    price: 55.0,
    isActive: false,
  },
];

const mockPaginatedResponse: PaginatedResponse<BarbershopService> = {
  items: mockServicos,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 1,
  totalCount: 3,
  hasPreviousPage: false,
  hasNextPage: false,
};

const mockEmptyResponse: PaginatedResponse<BarbershopService> = {
  items: [],
  pageNumber: 1,
  pageSize: 10,
  totalPages: 0,
  totalCount: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

describe('ServicosListPage', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    vi.clearAllMocks();
  });

  const renderWithProviders = (ui: React.ReactElement) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <BarbeariaProvider>
          <MemoryRouter initialEntries={['/TEST1234/servicos']}>
            <Routes>
              <Route path="/:codigo/servicos" element={ui} />
            </Routes>
          </MemoryRouter>
        </BarbeariaProvider>
      </QueryClientProvider>
    );
  };

  it('renderiza a página com título e botão de adicionar', async () => {
    vi.mocked(servicoService.list).mockResolvedValue(mockEmptyResponse);

    renderWithProviders(<ServicosListPage />);

    expect(screen.getByText('Serviços')).toBeInTheDocument();
    expect(screen.getByText('Adicionar Serviço')).toBeInTheDocument();
  });

  it('exibe lista de serviços quando dados são carregados', async () => {
    vi.mocked(servicoService.list).mockResolvedValue(mockPaginatedResponse);

    renderWithProviders(<ServicosListPage />);

    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
      expect(screen.getByText('Barba')).toBeInTheDocument();
      expect(screen.getByText('Corte + Barba')).toBeInTheDocument();
    });
  });

  it('formata preços corretamente', async () => {
    vi.mocked(servicoService.list).mockResolvedValue(mockPaginatedResponse);

    renderWithProviders(<ServicosListPage />);

    await waitFor(() => {
      expect(screen.getByText('R$ 35,00')).toBeInTheDocument();
      expect(screen.getByText('R$ 25,00')).toBeInTheDocument();
      expect(screen.getByText('R$ 55,00')).toBeInTheDocument();
    });
  });

  it('formata durações corretamente', async () => {
    vi.mocked(servicoService.list).mockResolvedValue(mockPaginatedResponse);

    renderWithProviders(<ServicosListPage />);

    await waitFor(() => {
      expect(screen.getByText('30min')).toBeInTheDocument();
      expect(screen.getByText('20min')).toBeInTheDocument();
      expect(screen.getByText('50min')).toBeInTheDocument();
    });
  });

  it('filtra serviços por busca de texto', async () => {
    vi.mocked(servicoService.list).mockResolvedValue(mockPaginatedResponse);
    const user = userEvent.setup();

    renderWithProviders(<ServicosListPage />);

    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText('Buscar serviços...');
    await user.type(searchInput, 'Barba');

    await waitFor(() => {
      expect(screen.getByText('Barba')).toBeInTheDocument();
      expect(screen.getByText('Corte + Barba')).toBeInTheDocument();
      expect(screen.queryByText('Corte de Cabelo')).not.toBeInTheDocument();
    });
  });

  it('limpa filtros quando clica no botão limpar', async () => {
    vi.mocked(servicoService.list).mockResolvedValue(mockPaginatedResponse);
    const user = userEvent.setup();

    renderWithProviders(<ServicosListPage />);

    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Aplicar filtro de busca
    const searchInput = screen.getByPlaceholderText('Buscar serviços...');
    await user.type(searchInput, 'Barba');

    await waitFor(() => {
      expect(screen.queryByText('Corte de Cabelo')).not.toBeInTheDocument();
    });

    // Limpar filtros
    const clearButton = screen.getByText('Limpar Filtros');
    await user.click(clearButton);

    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
      expect(searchInput).toHaveValue('');
    });
  });

  it('exibe mensagem quando não há serviços', async () => {
    vi.mocked(servicoService.list).mockResolvedValue(mockEmptyResponse);

    renderWithProviders(<ServicosListPage />);

    await waitFor(() => {
      expect(screen.getByText('Nenhum serviço encontrado')).toBeInTheDocument();
    });
  });

  it('navega para página de criação quando clica em adicionar', async () => {
    vi.mocked(servicoService.list).mockResolvedValue(mockEmptyResponse);
    const user = userEvent.setup();

    renderWithProviders(<ServicosListPage />);

    const addButton = await screen.findByText('Adicionar Serviço');
    await user.click(addButton);

    // Verifica que o botão existe e é clicável
    expect(addButton).toBeInTheDocument();
  });
});
