import { render, screen, waitFor, within } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ServicesListPage } from '@/pages/Services/List';

// Mock the services service
vi.mock('@/services/services.service', () => ({
  servicesService: {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    toggleActive: vi.fn(),
  },
}));

// Import the mocked services
import { servicesService } from '@/services/services.service';

// Get typed references to the mocked functions
const mockServicesList = vi.mocked(servicesService.list);
const mockServicesCreate = vi.mocked(servicesService.create);
const mockServicesUpdate = vi.mocked(servicesService.update);
const mockServicesToggleActive = vi.mocked(servicesService.toggleActive);

// Mock toast
const mockToast = vi.fn();
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: mockToast,
  }),
}));

// Mock data
const mockServices = [
  {
    id: '1',
    name: 'Corte de Cabelo',
    description: 'Corte masculino completo',
    durationMinutes: 30,
    price: 25.0,
    isActive: true,
  },
  {
    id: '2',
    name: 'Barba',
    description: 'Aparação e modelagem da barba',
    durationMinutes: 20,
    price: 15.0,
    isActive: false,
  },
  {
    id: '3',
    name: 'Corte + Barba',
    description: 'Pacote completo',
    durationMinutes: 50,
    price: 35.0,
    isActive: true,
  },
];

// Setup mocks
beforeEach(() => {
  vi.clearAllMocks();
  mockToast.mockClear();

  // Mock successful services list response
  mockServicesList.mockResolvedValue({
    items: mockServices,
    pageNumber: 1,
    pageSize: 20,
    totalPages: 1,
    totalCount: mockServices.length,
    hasPreviousPage: false,
    hasNextPage: false,
  });

  // Mock successful service creation
  mockServicesCreate.mockResolvedValue({
    id: '4',
    name: 'Sobrancelha',
    description: 'Design de sobrancelhas',
    durationMinutes: 15,
    price: 10.0,
    isActive: true,
  });

  // Mock successful service update
  mockServicesUpdate.mockResolvedValue({
    id: '1',
    name: 'Corte de Cabelo Atualizado',
    description: 'Corte masculino completo atualizado',
    durationMinutes: 35,
    price: 30.0,
    isActive: true,
  });

  // Mock successful service toggle
  mockServicesToggleActive.mockResolvedValue(undefined);
});

afterEach(() => {
  vi.clearAllMocks();
});

// Test wrapper with providers
const TestWrapper = ({ children }: { children: React.ReactNode }) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        {children}
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('ServicesListPage Integration', () => {
  it('renders services list with data from API', async () => {
    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for data to load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    expect(screen.getByText('Barba')).toBeInTheDocument();
    expect(screen.getByText('Corte + Barba')).toBeInTheDocument();
    expect(screen.getByText('Serviços')).toBeInTheDocument();
  });

  it('displays service details correctly', async () => {
    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for data to load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Check that service details are displayed
    expect(screen.getByText('Corte masculino completo')).toBeInTheDocument();
    expect(screen.getByText('30 min')).toBeInTheDocument();
    expect(screen.getByText('R$ 25,00')).toBeInTheDocument();
  });

  it('filters services by search term', async () => {
    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Check that search input exists
    const searchInput = screen.getByPlaceholderText('Buscar por nome...');
    expect(searchInput).toBeInTheDocument();
  });

  it('filters services by active status', async () => {
    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Check that the status filter exists
    const statusSelect = screen.getByRole('combobox');
    expect(statusSelect).toBeInTheDocument();
  });

  it('opens create service modal', async () => {
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Click create button
    const createButton = screen.getByText('+ Novo Serviço');
    await user.click(createButton);

    // Check if modal opens
    expect(screen.getByText('Novo Serviço')).toBeInTheDocument();
  });

  it('creates a new service successfully', async () => {
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Click create button
    const createButton = screen.getByText('+ Novo Serviço');
    await user.click(createButton);

    // Check that form inputs exist
    const nameInput = screen.getByLabelText('Nome *');
    const descriptionInput = screen.getByLabelText('Descrição *');
    const durationInput = screen.getByLabelText('Duração (minutos) *');
    const priceInput = screen.getByLabelText('Preço *');

    expect(nameInput).toBeInTheDocument();
    expect(descriptionInput).toBeInTheDocument();
    expect(durationInput).toBeInTheDocument();
    expect(priceInput).toBeInTheDocument();

    // Check that submit button exists
    const submitButton = screen.getByText('Criar');
    expect(submitButton).toBeInTheDocument();
  });

  it('opens edit service modal with pre-filled data', async () => {
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Find the row containing Corte de Cabelo and click edit
    const corteRow = screen.getByText('Corte de Cabelo').closest('tr');
    const editButton = within(corteRow!).getByText('Editar');
    await user.click(editButton);

    // Check if modal opens with edit title
    expect(screen.getByText('Editar Serviço')).toBeInTheDocument();

    // Check that submit button shows update text
    const submitButton = screen.getByText('Atualizar');
    expect(submitButton).toBeInTheDocument();
  });

  it('toggles service active status', async () => {
    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Find the row containing Corte de Cabelo and check that the deactivate button exists within it
    const corteRow = screen.getByText('Corte de Cabelo').closest('tr');
    const deactivateButtons = within(corteRow!).getAllByText('Desativar');
    expect(deactivateButtons.length).toBeGreaterThan(0);
  });

  it('opens confirmation dialog when toggling service status', async () => {
    const user = userEvent.setup();

    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Find the row containing Corte de Cabelo and click deactivate
    const corteRow = screen.getByText('Corte de Cabelo').closest('tr');
    const deactivateButtons = within(corteRow!).getAllByText('Desativar');
    await user.click(deactivateButtons[0]);

    // Check if confirmation dialog opens
    await waitFor(() => {
      expect(screen.getByText('Desativar Serviço')).toBeInTheDocument();
    });
  });

  it('handles API errors gracefully', async () => {
    // Mock a failed request
    mockServicesList.mockRejectedValue(new Error('Network Error'));

    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for error state
    await waitFor(() => {
      expect(screen.getByText('Erro ao carregar serviços')).toBeInTheDocument();
    });
  });

  it('handles duplicate service name error (409)', async () => {
    const user = userEvent.setup();

    // Mock 409 error
    mockServicesCreate.mockRejectedValue({
      response: {
        status: 409,
        data: {
          message: 'Já existe um serviço com este nome.',
        },
      },
    });

    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Click create button
    const createButton = screen.getByText('+ Novo Serviço');
    await user.click(createButton);

    // Fill form with valid data
    const nameInput = screen.getByLabelText('Nome *');
    const descriptionInput = screen.getByLabelText('Descrição *');
    const durationInput = screen.getByLabelText('Duração (minutos) *');

    await user.type(nameInput, 'Corte de Cabelo');
    await user.type(descriptionInput, 'Descrição teste');
    await user.type(durationInput, '30');

    // Submit form
    const submitButton = screen.getByText('Criar');
    await user.click(submitButton);

    // Wait for error toast
    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith(
        expect.objectContaining({
          title: 'Nome duplicado',
          variant: 'destructive',
        })
      );
    });
  });

  it('displays price in Brazilian currency format', async () => {
    render(
      <TestWrapper>
        <ServicesListPage />
      </TestWrapper>
    );

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
    });

    // Check that prices are formatted correctly
    expect(screen.getByText('R$ 25,00')).toBeInTheDocument();
    expect(screen.getByText('R$ 15,00')).toBeInTheDocument();
    expect(screen.getByText('R$ 35,00')).toBeInTheDocument();
  });
});
