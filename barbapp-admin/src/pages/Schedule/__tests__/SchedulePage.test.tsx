import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { SchedulePage } from '../SchedulePage';
import { AppointmentStatus } from '@/types/schedule';

// Mock hooks
vi.mock('@/hooks', () => ({
  useSchedule: vi.fn(),
  useBarbers: vi.fn(),
}));

import { useSchedule, useBarbers } from '@/hooks';

const mockUseSchedule = vi.mocked(useSchedule);
const mockUseBarbers = vi.mocked(useBarbers);

describe('SchedulePage', () => {
  let queryClient: QueryClient;
  const user = userEvent.setup();

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });

    // Default mock implementations
    mockUseBarbers.mockReturnValue({
      data: {
        items: [
          {
            id: '1',
            name: 'João Silva',
            email: 'joao@example.com',
            phoneFormatted: '(11) 98765-4321',
            services: [],
            isActive: true,
            createdAt: '2024-01-01T00:00:00Z',
          },
          {
            id: '2',
            name: 'Maria Santos',
            email: 'maria@example.com',
            phoneFormatted: '(11) 91234-5678',
            services: [],
            isActive: true,
            createdAt: '2024-01-01T00:00:00Z',
          },
        ],
        pageNumber: 1,
        pageSize: 100,
        totalPages: 1,
        totalCount: 2,
        hasPreviousPage: false,
        hasNextPage: false,
      },
      isLoading: false,
      error: null,
    } as any);

    mockUseSchedule.mockReturnValue({
      data: {
        appointments: [
          {
            id: '1',
            barberId: '1',
            barberName: 'João Silva',
            customerId: '1',
            customerName: 'Cliente 1',
            startTime: '2024-10-16T09:00:00Z',
            endTime: '2024-10-16T09:30:00Z',
            serviceName: 'Corte de Cabelo',
            status: AppointmentStatus.Confirmed,
          },
          {
            id: '2',
            barberId: '1',
            barberName: 'João Silva',
            customerId: '2',
            customerName: 'Cliente 2',
            startTime: '2024-10-16T10:00:00Z',
            endTime: '2024-10-16T10:30:00Z',
            serviceName: 'Barba',
            status: AppointmentStatus.Pending,
          },
        ],
      },
      isLoading: false,
      error: null,
    } as any);
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        {children}
      </MemoryRouter>
    </QueryClientProvider>
  );

  it('should render page title', () => {
    render(<SchedulePage />, { wrapper });
    expect(screen.getByText('Agenda')).toBeInTheDocument();
  });

  it('should render appointments list', async () => {
    render(<SchedulePage />, { wrapper });

    await waitFor(() => {
      expect(screen.getByText('Cliente 1')).toBeInTheDocument();
      expect(screen.getByText('Cliente 2')).toBeInTheDocument();
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
      expect(screen.getByText('Barba')).toBeInTheDocument();
    });
  });

  it('should group appointments by barber', async () => {
    render(<SchedulePage />, { wrapper });

    await waitFor(() => {
      // Should show barber name as grouping header
      expect(screen.getByText(/João Silva/)).toBeInTheDocument();
      // Should show appointment count
      expect(screen.getByText(/2 agendamentos/)).toBeInTheDocument();
    });
  });

  it('should render loading state', () => {
    mockUseSchedule.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    } as any);

    render(<SchedulePage />, { wrapper });

    // Should show skeleton loaders
    expect(document.querySelectorAll('.animate-pulse').length).toBeGreaterThan(0);
  });

  it('should render error state', () => {
    mockUseSchedule.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: new Error('Failed to fetch'),
    } as any);

    render(<SchedulePage />, { wrapper });

    expect(screen.getByText('Erro ao carregar agenda')).toBeInTheDocument();
  });

  it('should render empty state', () => {
    mockUseSchedule.mockReturnValue({
      data: {
        appointments: [],
      },
      isLoading: false,
      error: null,
    } as any);

    render(<SchedulePage />, { wrapper });

    expect(screen.getByText('Nenhum agendamento encontrado para esta data.')).toBeInTheDocument();
  });

  it('should render filter controls', () => {
    render(<SchedulePage />, { wrapper });

    expect(screen.getByLabelText('Data')).toBeInTheDocument();
    expect(screen.getByLabelText('Barbeiro')).toBeInTheDocument();
    expect(screen.getByLabelText('Status')).toBeInTheDocument();
  });

  it('should render navigation controls', () => {
    render(<SchedulePage />, { wrapper });

    expect(screen.getByLabelText('Dia anterior')).toBeInTheDocument();
    expect(screen.getByLabelText('Próximo dia')).toBeInTheDocument();
  });

  it('should render barber options in filter', async () => {
    render(<SchedulePage />, { wrapper });

    const barberSelect = screen.getByRole('combobox', { name: /barbeiro/i });
    await user.click(barberSelect);

    await waitFor(() => {
      // Should show barbers in dropdown
      const options = screen.getAllByText('João Silva');
      expect(options.length).toBeGreaterThan(0);
      const mariaOptions = screen.getAllByText('Maria Santos');
      expect(mariaOptions.length).toBeGreaterThan(0);
    });
  });

  it('should render status options in filter', async () => {
    render(<SchedulePage />, { wrapper });

    const statusSelect = screen.getByRole('combobox', { name: /status/i });
    await user.click(statusSelect);

    await waitFor(() => {
      // Should show status options in dropdown
      expect(screen.getAllByText('Pendente').length).toBeGreaterThan(0);
      expect(screen.getAllByText('Confirmado').length).toBeGreaterThan(0);
      expect(screen.getAllByText('Cancelado').length).toBeGreaterThan(0);
      expect(screen.getAllByText('Concluído').length).toBeGreaterThan(0);
    });
  });

  it('should display appointment status badges', async () => {
    render(<SchedulePage />, { wrapper });

    await waitFor(() => {
      expect(screen.getByText('Confirmado')).toBeInTheDocument();
      expect(screen.getByText('Pendente')).toBeInTheDocument();
    });
  });

  it('should format times correctly', async () => {
    render(<SchedulePage />, { wrapper });

    await waitFor(() => {
      // Times should be formatted in pt-BR
      const timeElements = screen.getAllByText(/\d{2}:\d{2}/);
      expect(timeElements.length).toBeGreaterThan(0);
    });
  });
});
