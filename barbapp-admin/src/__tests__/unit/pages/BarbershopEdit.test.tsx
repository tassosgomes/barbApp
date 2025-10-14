import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BarbershopEdit } from '@/pages/Barbershops/Edit';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop } from '@/types';

// Mock dependencies
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    getById: vi.fn(),
    update: vi.fn(),
  },
}));

vi.mock('react-router-dom', () => {
  const navigateMock = vi.fn();
  return {
    useParams: () => ({ id: '1' }),
    useNavigate: () => navigateMock,
  };
});

// Mock the form component
vi.mock('@/components/barbershop/BarbershopEditForm', () => ({
  BarbershopEditForm: () => null,
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

describe('BarbershopEdit', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should load barbershop data on mount', async () => {
    (barbershopService.getById as any).mockResolvedValue(mockBarbershop); // eslint-disable-line @typescript-eslint/no-explicit-any

    render(<BarbershopEdit />);

    await waitFor(() => {
      expect(barbershopService.getById).toHaveBeenCalledWith('1');
    });
  });

  it('should show loading state initially', () => {
    (barbershopService.getById as any).mockImplementation(() => new Promise(() => {})); // eslint-disable-line @typescript-eslint/no-explicit-any

    render(<BarbershopEdit />);

    expect(screen.getByText('Carregando...')).toBeInTheDocument();
  });

  it('should render edit form with correct title', async () => {
    (barbershopService.getById as any).mockResolvedValue(mockBarbershop); // eslint-disable-line @typescript-eslint/no-explicit-any

    render(<BarbershopEdit />);

    await waitFor(() => {
      expect(screen.getByText('Editar Barbearia')).toBeInTheDocument();
    });

    expect(screen.getByText('Cancelar')).toBeInTheDocument();
    expect(screen.getByText('Salvar Alterações')).toBeInTheDocument();
  });
});