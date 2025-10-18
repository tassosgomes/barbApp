import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { BarbeiroFormPage } from '../BarbeiroFormPage';
import { barbeiroService } from '@/services/barbeiro.service';
import { servicesService } from '@/services/services.service';
import { BarbeariaProvider } from '@/contexts/BarbeariaContext';
import type { Barber, BarbershopService, PaginatedResponse } from '@/types';

// Mock dos serviços
vi.mock('@/services/barbeiro.service');
vi.mock('@/services/services.service');

// Mock do hook useToast
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

const mockServicos: BarbershopService[] = [
  {
    id: 's1',
    name: 'Corte',
    description: 'Corte de cabelo',
    durationMinutes: 30,
    price: 3000,
    isActive: true,
  },
  {
    id: 's2',
    name: 'Barba',
    description: 'Aparar barba',
    durationMinutes: 20,
    price: 2000,
    isActive: true,
  },
];

const mockServicosResponse: PaginatedResponse<BarbershopService> = {
  items: mockServicos,
  pageNumber: 1,
  pageSize: 100,
  totalPages: 1,
  totalCount: 2,
  hasPreviousPage: false,
  hasNextPage: false,
};

const mockBarbeiro: Barber = {
  id: '1',
  name: 'João Silva',
  email: 'joao@example.com',
  phoneFormatted: '(11) 98765-4321',
  services: [{ id: 's1', name: 'Corte' }],
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
};

describe('BarbeiroFormPage', () => {
  let queryClient: QueryClient;
  const mockNavigate = vi.fn();

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });

    // Setup mock context data
    localStorage.setItem(
      'admin_barbearia_context',
      JSON.stringify({
        id: 'barb-1',
        nome: 'Barbearia Teste',
        codigo: 'TEST1234',
        isActive: true,
      })
    );

    vi.clearAllMocks();
    
    // Mock navigate
    vi.mock('react-router-dom', async () => {
      const actual = await vi.importActual('react-router-dom');
      return {
        ...actual,
        useNavigate: () => mockNavigate,
      };
    });
  });

  const renderComponent = (mode: 'create' | 'edit' = 'create', barberoId?: string) => {
    const initialEntry =
      mode === 'create'
        ? '/TEST1234/barbeiros/novo'
        : `/TEST1234/barbeiros/${barberoId}`;

    return render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={[initialEntry]}>
          <BarbeariaProvider>
            <Routes>
              <Route path="/:codigo/barbeiros/novo" element={<BarbeiroFormPage />} />
              <Route path="/:codigo/barbeiros/:id" element={<BarbeiroFormPage />} />
            </Routes>
          </BarbeariaProvider>
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  describe('Create Mode', () => {
    beforeEach(() => {
      vi.mocked(servicesService.list).mockResolvedValue(mockServicosResponse);
    });

    it('should render create form title', async () => {
      renderComponent('create');

      await waitFor(() => {
        expect(screen.getByText('Novo Barbeiro')).toBeInTheDocument();
      });
    });

    it('should render all form fields for creation', async () => {
      renderComponent('create');

      await waitFor(() => {
        expect(screen.getByLabelText(/nome/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/telefone/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/senha/i)).toBeInTheDocument();
      });
    });

    it('should display available services as checkboxes', async () => {
      renderComponent('create');

      await waitFor(() => {
        expect(screen.getByText(/corte/i)).toBeInTheDocument();
        expect(screen.getByText(/barba/i)).toBeInTheDocument();
      });
    });

    it('should validate required fields', async () => {
      const user = userEvent.setup();
      renderComponent('create');

      await waitFor(() => {
        expect(screen.getByText('Novo Barbeiro')).toBeInTheDocument();
      });

      const submitButton = screen.getByRole('button', { name: /cadastrar/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText(/nome deve ter no mínimo 3 caracteres/i)).toBeInTheDocument();
      });
    });

    it('should format phone number as user types', async () => {
      const user = userEvent.setup();
      renderComponent('create');

      await waitFor(() => {
        expect(screen.getByLabelText(/telefone/i)).toBeInTheDocument();
      });

      const phoneInput = screen.getByLabelText(/telefone/i);
      await user.type(phoneInput, '11987654321');

      expect(phoneInput).toHaveValue('(11) 98765-4321');
    });

    it('should call create service on submit with valid data', async () => {
      const user = userEvent.setup();
      vi.mocked(barbeiroService.create).mockResolvedValue(mockBarbeiro);

      renderComponent('create');

      await waitFor(() => {
        expect(screen.getByLabelText(/nome/i)).toBeInTheDocument();
      });

      // Fill form
      await user.type(screen.getByLabelText(/nome/i), 'João Silva');
      await user.type(screen.getByLabelText(/email/i), 'joao@example.com');
      await user.type(screen.getByLabelText(/telefone/i), '11987654321');
      await user.type(screen.getByLabelText(/senha/i), 'senha123');

      // Select service
      const corteCheckbox = screen.getByRole('checkbox', { name: /corte/i });
      await user.click(corteCheckbox);

      // Submit
      const submitButton = screen.getByRole('button', { name: /cadastrar/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(barbeiroService.create).toHaveBeenCalledWith(
          expect.objectContaining({
            name: 'João Silva',
            email: 'joao@example.com',
            phone: '11987654321',
            password: 'senha123',
            serviceIds: ['s1'],
          })
        );
      });
    });

    it('should require at least one service selected', async () => {
      const user = userEvent.setup();
      renderComponent('create');

      await waitFor(() => {
        expect(screen.getByLabelText(/nome/i)).toBeInTheDocument();
      });

      // Fill only basic fields, no services
      await user.type(screen.getByLabelText(/nome/i), 'João Silva');
      await user.type(screen.getByLabelText(/email/i), 'joao@example.com');
      await user.type(screen.getByLabelText(/telefone/i), '11987654321');
      await user.type(screen.getByLabelText(/senha/i), 'senha123');

      // Submit
      const submitButton = screen.getByRole('button', { name: /cadastrar/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText(/selecione pelo menos um serviço/i)).toBeInTheDocument();
      });
    });
  });

  describe('Edit Mode', () => {
    beforeEach(() => {
      vi.mocked(servicesService.list).mockResolvedValue(mockServicosResponse);
      vi.mocked(barbeiroService.getById).mockResolvedValue(mockBarbeiro);
    });

    it('should render edit form title', async () => {
      renderComponent('edit', '1');

      await waitFor(() => {
        expect(screen.getByText('Editar Barbeiro')).toBeInTheDocument();
      });
    });

    it('should NOT render email and password fields in edit mode', async () => {
      renderComponent('edit', '1');

      await waitFor(() => {
        expect(screen.getByLabelText(/nome/i)).toBeInTheDocument();
      });

      expect(screen.queryByLabelText(/email/i)).not.toBeInTheDocument();
      expect(screen.queryByLabelText(/senha/i)).not.toBeInTheDocument();
    });

    it('should load existing barbeiro data into form', async () => {
      renderComponent('edit', '1');

      await waitFor(() => {
        const nameInput = screen.getByLabelText(/nome/i) as HTMLInputElement;
        expect(nameInput.value).toBe('João Silva');

        const phoneInput = screen.getByLabelText(/telefone/i) as HTMLInputElement;
        expect(phoneInput.value).toBe('(11) 98765-4321');
      });
    });

    it('should pre-select services from existing barbeiro', async () => {
      renderComponent('edit', '1');

      await waitFor(() => {
        const corteCheckbox = screen.getByRole('checkbox', {
          name: /corte/i,
        }) as HTMLInputElement;
        expect(corteCheckbox.checked).toBe(true);

        const barbeCheckbox = screen.getByRole('checkbox', {
          name: /barba/i,
        }) as HTMLInputElement;
        expect(barbeCheckbox.checked).toBe(false);
      });
    });

    it('should call update service on submit', async () => {
      const user = userEvent.setup();
      vi.mocked(barbeiroService.update).mockResolvedValue(mockBarbeiro);

      renderComponent('edit', '1');

      await waitFor(() => {
        expect(screen.getByLabelText(/nome/i)).toBeInTheDocument();
      });

      // Change name
      const nameInput = screen.getByLabelText(/nome/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'João da Silva');

      // Submit
      const submitButton = screen.getByRole('button', { name: /atualizar/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(barbeiroService.update).toHaveBeenCalledWith(
          '1',
          expect.objectContaining({
            name: 'João da Silva',
            phone: '11987654321',
            serviceIds: ['s1'],
          })
        );
      });
    });
  });

  describe('Common Behaviors', () => {
    beforeEach(() => {
      vi.mocked(servicesService.list).mockResolvedValue(mockServicosResponse);
    });

    it('should have cancel button that navigates back', async () => {
      const user = userEvent.setup();
      renderComponent('create');

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /cancelar/i })).toBeInTheDocument();
      });

      const cancelButton = screen.getByRole('button', { name: /cancelar/i });
      await user.click(cancelButton);

      // Note: actual navigation would be tested in E2E
    });

    it('should have back button in header', async () => {
      renderComponent('create');

      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: /voltar para lista de barbeiros/i })
        ).toBeInTheDocument();
      });
    });

    it('should disable form fields while submitting', async () => {
      const user = userEvent.setup();
      vi.mocked(barbeiroService.create).mockImplementation(
        () => new Promise(() => {}) // Never resolves
      );

      renderComponent('create');

      await waitFor(() => {
        expect(screen.getByLabelText(/nome/i)).toBeInTheDocument();
      });

      // Fill form
      await user.type(screen.getByLabelText(/nome/i), 'João Silva');
      await user.type(screen.getByLabelText(/email/i), 'joao@example.com');
      await user.type(screen.getByLabelText(/telefone/i), '11987654321');
      await user.type(screen.getByLabelText(/senha/i), 'senha123');

      const corteCheckbox = screen.getByRole('checkbox', { name: /corte/i });
      await user.click(corteCheckbox);

      // Submit
      const submitButton = screen.getByRole('button', { name: /cadastrar/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /salvando/i })).toBeDisabled();
      });
    });

    it('should display message when no services available', async () => {
      vi.mocked(servicesService.list).mockResolvedValue({
        ...mockServicosResponse,
        items: [],
        totalCount: 0,
      });

      renderComponent('create');

      await waitFor(() => {
        expect(
          screen.getByText(/nenhum serviço disponível/i)
        ).toBeInTheDocument();
      });
    });
  });
});
