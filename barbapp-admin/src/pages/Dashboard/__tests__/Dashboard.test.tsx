import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { Dashboard } from '../Dashboard';
import { BarbeariaProvider } from '@/contexts/BarbeariaContext';
import * as dashboardService from '@/services/dashboard.service';

// Mock dashboard service
vi.mock('@/services/dashboard.service', () => ({
  dashboardService: {
    getMetrics: vi.fn(),
  },
}));

const createQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

const renderWithProviders = (ui: React.ReactElement) => {
  const queryClient = createQueryClient();

  // Setup barbearia context
  localStorage.setItem(
    'admin_barbearia_context',
    JSON.stringify({
      id: '00000000-0000-0000-0000-000000000000',
      nome: 'Barbearia Teste',
      codigo: 'TEST1234',
      isActive: true,
    })
  );

  return render(
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>
        <BarbeariaProvider>{ui}</BarbeariaProvider>
      </QueryClientProvider>
    </BrowserRouter>
  );
};

describe('Dashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('should render metrics cards when data is loaded', async () => {
    const mockMetrics = {
      totalBarbeiros: 5,
      totalServicos: 8,
      agendamentosHoje: 12,
      proximosAgendamentos: [],
    };

    vi.mocked(dashboardService.dashboardService.getMetrics).mockResolvedValue(mockMetrics);

    renderWithProviders(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText('Total de Barbeiros')).toBeInTheDocument();
    });

    expect(screen.getByText('Serviços Ativos')).toBeInTheDocument();
    expect(screen.getByText('Agendamentos Hoje')).toBeInTheDocument();
  });

  it('should render upcoming appointments when available', async () => {
    const mockMetrics = {
      totalBarbeiros: 5,
      totalServicos: 8,
      agendamentosHoje: 2,
      proximosAgendamentos: [
        {
          id: '1',
          clienteNome: 'João Silva',
          barbeiro: 'Pedro Barbeiro',
          servico: 'Corte de Cabelo',
          data: '18/10/2025',
          hora: '10:00',
          status: 'Confirmado',
        },
      ],
    };

    vi.mocked(dashboardService.dashboardService.getMetrics).mockResolvedValue(mockMetrics);

    renderWithProviders(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    expect(screen.getByText(/Pedro Barbeiro/)).toBeInTheDocument();
    expect(screen.getByText(/Corte de Cabelo/)).toBeInTheDocument();
  });

  it('should render empty state when no appointments', async () => {
    const mockMetrics = {
      totalBarbeiros: 5,
      totalServicos: 8,
      agendamentosHoje: 0,
      proximosAgendamentos: [],
    };

    vi.mocked(dashboardService.dashboardService.getMetrics).mockResolvedValue(mockMetrics);

    renderWithProviders(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/Nenhum agendamento próximo para hoje/i)).toBeInTheDocument();
    });
  });

  it('should render welcome header', async () => {
    const mockMetrics = {
      totalBarbeiros: 0,
      totalServicos: 0,
      agendamentosHoje: 0,
      proximosAgendamentos: [],
    };

    vi.mocked(dashboardService.dashboardService.getMetrics).mockResolvedValue(mockMetrics);

    renderWithProviders(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/Bem-vindo ao painel!/i)).toBeInTheDocument();
    });

    expect(
      screen.getByText(/Gerencie sua barbearia de forma simples e eficiente/i)
    ).toBeInTheDocument();
  });
});
