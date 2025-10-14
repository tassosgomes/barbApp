import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BarbershopCreate } from '@/pages/Barbershops/Create';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { barbershopService } from '@/services/barbershop.service';

// Mock dependencies
vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    create: vi.fn(),
  },
}));

vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

vi.mock('@/components/barbershop/BarbershopForm', () => ({
  BarbershopForm: () => (
    <div data-testid="barbershop-form">
      {/* Mock form fields to pass validation */}
    </div>
  ),
}));

vi.mock('react-hook-form', () => ({
  useForm: () => ({
    register: vi.fn(),
    handleSubmit: (fn: any) => (e?: any) => {
      e?.preventDefault?.();
      // Mock valid form data
      const mockData = {
        name: 'Test Barbershop',
        document: '12345678901',
        ownerName: 'John Doe',
        email: 'john@example.com',
        phone: '11987654321',
        address: {
          zipCode: '12345-678',
          street: 'Test Street',
          number: '123',
          complement: '',
          neighborhood: 'Test Neighborhood',
          city: 'Test City',
          state: 'SP',
        },
      };
      return fn(mockData);
    },
    formState: { errors: {} },
    setValue: vi.fn(),
    watch: vi.fn(),
  }),
}));

vi.mock('@hookform/resolvers/zod', () => ({
  zodResolver: vi.fn(),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: vi.fn(),
  };
});

describe('BarbershopCreate', () => {
  const mockCreate = vi.mocked(barbershopService.create);
  let mockNavigate: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    vi.clearAllMocks();
    mockNavigate = vi.fn();
    // Mock useNavigate to return our spy
    const { useNavigate } = vi.mocked(await import('react-router-dom'));
    useNavigate.mockReturnValue(mockNavigate);
  });

  it('renders create form initially', () => {
    render(
      <MemoryRouter>
        <BarbershopCreate />
      </MemoryRouter>
    );

    expect(screen.getByText('Nova Barbearia')).toBeInTheDocument();
    expect(screen.getByTestId('barbershop-form')).toBeInTheDocument();
    expect(screen.getByText('Cancelar')).toBeInTheDocument();
    expect(screen.getByText('Salvar')).toBeInTheDocument();
  });

  it('submits form and shows success state', async () => {
    const user = userEvent.setup();
    const mockBarbershop = {
      id: '1',
      code: 'ABC123',
      name: 'Test Barbershop',
      document: '12345678901',
      ownerName: 'John Doe',
      email: 'john@example.com',
      phone: '11987654321',
      isActive: true,
      address: {
        zipCode: '12345-678',
        street: 'Test Street',
        number: '123',
        complement: '',
        neighborhood: 'Test Neighborhood',
        city: 'Test City',
        state: 'SP',
      },
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    mockCreate.mockResolvedValueOnce(mockBarbershop);

    render(
      <MemoryRouter>
        <BarbershopCreate />
      </MemoryRouter>
    );

    const submitButton = screen.getByText('Salvar');
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Barbearia Criada com Sucesso!')).toBeInTheDocument();
    });

    expect(screen.getByText('ABC123')).toBeInTheDocument();
    expect(screen.getByText('Voltar à Lista')).toBeInTheDocument();
    expect(screen.getByText('Criar Outra')).toBeInTheDocument();
  });

  it('handles form submission error', async () => {
    const user = userEvent.setup();
    const errorMessage = 'Erro ao criar barbearia';

    mockCreate.mockRejectedValueOnce(new Error(errorMessage));

    render(
      <MemoryRouter>
        <BarbershopCreate />
      </MemoryRouter>
    );

    const submitButton = screen.getByText('Salvar');
    await user.click(submitButton);

    // Form should still be visible after error
    await waitFor(() => {
      expect(screen.getByText('Nova Barbearia')).toBeInTheDocument();
    });
  });

  it('navigates back when cancel is clicked', async () => {
    const user = userEvent.setup();

    render(
      <MemoryRouter>
        <BarbershopCreate />
      </MemoryRouter>
    );

    const cancelButton = screen.getByText('Cancelar');
    await user.click(cancelButton);

    expect(mockNavigate).toHaveBeenCalledWith(-1);
  });

  it('allows creating another barbershop after success', async () => {
    const user = userEvent.setup();
    const mockBarbershop = {
      id: '1',
      code: 'ABC123',
      name: 'Test Barbershop',
      document: '12345678901',
      ownerName: 'John Doe',
      email: 'john@example.com',
      phone: '11987654321',
      isActive: true,
      address: {
        zipCode: '12345-678',
        street: 'Test Street',
        number: '123',
        complement: '',
        neighborhood: 'Test Neighborhood',
        city: 'Test City',
        state: 'SP',
      },
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    mockCreate.mockResolvedValueOnce(mockBarbershop);

    render(
      <MemoryRouter>
        <BarbershopCreate />
      </MemoryRouter>
    );

    // Submit form
    const submitButton = screen.getByText('Salvar');
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Barbearia Criada com Sucesso!')).toBeInTheDocument();
    });

    // Click "Criar Outra"
    const createAnotherButton = screen.getByText('Criar Outra');
    await user.click(createAnotherButton);

    // Should go back to form
    expect(screen.getByText('Nova Barbearia')).toBeInTheDocument();
  });

  it('navigates to list when voltar button is clicked', async () => {
    const user = userEvent.setup();
    const mockBarbershop = {
      id: '1',
      code: 'ABC123',
      name: 'Test Barbershop',
      document: '12345678901',
      ownerName: 'John Doe',
      email: 'john@example.com',
      phone: '11987654321',
      isActive: true,
      address: {
        zipCode: '12345-678',
        street: 'Test Street',
        number: '123',
        complement: '',
        neighborhood: 'Test Neighborhood',
        city: 'Test City',
        state: 'SP',
      },
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    mockCreate.mockResolvedValueOnce(mockBarbershop);

    render(
      <MemoryRouter>
        <BarbershopCreate />
      </MemoryRouter>
    );

    // Submit form
    const submitButton = screen.getByText('Salvar');
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Barbearia Criada com Sucesso!')).toBeInTheDocument();
    });

    // Click "Voltar à Lista"
    const backToListButton = screen.getByText('Voltar à Lista');
    await user.click(backToListButton);

    expect(mockNavigate).toHaveBeenCalledWith('/barbearias');
  });
});