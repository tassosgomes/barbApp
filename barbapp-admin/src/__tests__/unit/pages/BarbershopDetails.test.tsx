import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BarbershopDetails } from '@/pages/Barbershops/Details';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { barbershopService } from '@/services/barbershop.service';
import { copyToClipboard } from '@/lib/utils';

// Mock dependencies
vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    getById: vi.fn(),
    deactivate: vi.fn(),
    reactivate: vi.fn(),
  },
}));

vi.mock('@/hooks/use-toast', () => ({
  useToast: vi.fn(),
}));

vi.mock('@/lib/utils', async (importOriginal) => {
  const actual = await importOriginal() as any;
  return {
    ...actual,
    copyToClipboard: vi.fn(),
  };
});

vi.mock('@/utils/formatters', () => ({
  formatDate: vi.fn((date) => `formatted-${date}`),
  applyDocumentMask: vi.fn((doc) => `masked-${doc}`),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: vi.fn(),
    useParams: vi.fn(() => ({ id: '1' })),
  };
});

// Import mocked functions
const mockUseNavigate = vi.mocked(await import('react-router-dom')).useNavigate;
const mockUseToast = vi.mocked(await import('@/hooks/use-toast')).useToast;

describe('BarbershopDetails', () => {
  const mockGetById = vi.mocked(barbershopService.getById);
  const mockDeactivate = vi.mocked(barbershopService.deactivate);
  const mockReactivate = vi.mocked(barbershopService.reactivate);
  const mockNavigate = vi.fn();
  const mockToast = vi.fn();
  const mockDismiss = vi.fn();

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
      complement: 'Apt 4',
      neighborhood: 'Test Neighborhood',
      city: 'Test City',
      state: 'SP',
    },
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
  };

  beforeEach(() => {
    mockNavigate.mockClear();
    mockToast.mockClear();
    mockDismiss.mockClear();
    mockUseNavigate.mockReturnValue(mockNavigate);
    mockUseToast.mockReturnValue({ 
      toast: mockToast, 
      dismiss: mockDismiss, 
      toasts: [] 
    });
  });

  it('shows loading state initially', async () => {
    mockGetById.mockImplementation(() => new Promise(() => {})); // Never resolves

    render(
      <MemoryRouter>
        <BarbershopDetails />
      </MemoryRouter>
    );

    // Check for skeleton loading indicators
    expect(document.querySelectorAll('.animate-pulse').length).toBeGreaterThan(0);
  });

  it('displays barbershop details when loaded successfully', async () => {
    mockGetById.mockReturnValue(new Promise(resolve => setTimeout(() => resolve(mockBarbershop), 0)));

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    // Wait for the API call to be made
    await waitFor(() => {
      expect(mockGetById).toHaveBeenCalledWith('1');
    }, { timeout: 1000 });

    // Wait for loading to complete - check that skeleton is gone
    await waitFor(() => {
      expect(document.querySelectorAll('.animate-pulse')).toHaveLength(0);
    }, { timeout: 5000 });

    expect(screen.getByText('ABC123')).toBeInTheDocument();
    expect(screen.getByText('masked-12345678901')).toBeInTheDocument();
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('john@example.com')).toBeInTheDocument();
    expect(screen.getByText('11987654321')).toBeInTheDocument();
    expect(screen.getByText('Test Street, 123 - Apt 4')).toBeInTheDocument();
    expect(screen.getByText('Test City - SP')).toBeInTheDocument();
    expect(screen.getByText('formatted-2024-01-01T00:00:00Z')).toBeInTheDocument();
    expect(screen.getByText('formatted-2024-01-02T00:00:00Z')).toBeInTheDocument();
  });

  it('navigates back when loading fails', async () => {
    mockGetById.mockReturnValue(new Promise((_, reject) => setTimeout(() => reject(new Error('API Error')), 0)));

    render(
      <MemoryRouter>
        <BarbershopDetails />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/barbearias');
    });
  });

  it('shows not found state when barbershop is null', async () => {
    mockGetById.mockReturnValue(new Promise(resolve => setTimeout(() => resolve(null as any), 0)));

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Barbearia não encontrada')).toBeInTheDocument();
    });

    expect(screen.getByText('A barbearia solicitada não existe ou foi removida.')).toBeInTheDocument();
  });

  it('copies code to clipboard successfully', async () => {
    mockGetById.mockReturnValue(new Promise(resolve => setTimeout(() => resolve(mockBarbershop), 0)));
    vi.mocked(copyToClipboard).mockReturnValue(new Promise(resolve => setTimeout(() => resolve(true), 0)));

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Test Barbershop');
    });

    const copyButton = screen.getByRole('button', { name: /copiar código/i });
    await userEvent.click(copyButton);

    expect(vi.mocked(copyToClipboard)).toHaveBeenCalledWith('ABC123');
  });

  it('handles copy code failure', async () => {
    mockGetById.mockReturnValue(new Promise(resolve => setTimeout(() => resolve(mockBarbershop), 0)));
    vi.mocked(copyToClipboard).mockReturnValue(new Promise(resolve => setTimeout(() => resolve(false), 0)));

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Test Barbershop');
    });

    const copyButton = screen.getByRole('button', { name: /copiar código/i });
    await userEvent.click(copyButton);

    expect(vi.mocked(copyToClipboard)).toHaveBeenCalledWith('ABC123');
  });

  it('deactivates active barbershop', async () => {
    mockGetById.mockResolvedValueOnce(mockBarbershop); // Initial load - active
    mockDeactivate.mockResolvedValueOnce(undefined);
    mockGetById.mockResolvedValueOnce({ ...mockBarbershop, isActive: false }); // After deactivation

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Test Barbershop');
    });

    const deactivateButton = screen.getByRole('button', { name: /desativar/i });
    await userEvent.click(deactivateButton);

    expect(mockDeactivate).toHaveBeenCalledWith('1');
  });

  it('reactivates inactive barbershop', async () => {
    const inactiveBarbershop = { ...mockBarbershop, isActive: false };
    mockGetById.mockReturnValue(new Promise(resolve => setTimeout(() => resolve(inactiveBarbershop), 0))); // Initial load
    mockReactivate.mockReturnValue(new Promise(resolve => setTimeout(() => resolve(undefined), 0)));
    // Note: After reactivation, the component will call getById again, but we'll keep the same mock

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Test Barbershop');
    });

    const reactivateButton = screen.getByRole('button', { name: /reativar/i });
    await userEvent.click(reactivateButton);

    expect(mockReactivate).toHaveBeenCalledWith('1');
  });

  it('handles status toggle error', async () => {
    mockGetById.mockResolvedValueOnce(mockBarbershop);
    mockDeactivate.mockRejectedValueOnce(new Error('API Error'));

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Test Barbershop');
    });

    const deactivateButton = screen.getByRole('button', { name: /desativar/i });
    await userEvent.click(deactivateButton);

    // Wait for the error to be handled
    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Erro ao alterar status',
        description: 'Não foi possível alterar o status da barbearia.',
        variant: 'destructive',
      });
    });

    expect(mockDeactivate).toHaveBeenCalledWith('1');
  });

  it('navigates back when voltar button is clicked', async () => {
    mockGetById.mockReturnValue(new Promise(resolve => setTimeout(() => resolve(mockBarbershop), 0)));

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Test Barbershop');
    });

    const backButton = screen.getByRole('button', { name: /voltar/i });
    await userEvent.click(backButton);

    expect(mockNavigate).toHaveBeenCalledWith(-1);
  });

  it('navigates to edit page when editar button is clicked', async () => {
    mockGetById.mockReturnValue(new Promise(resolve => setTimeout(() => resolve(mockBarbershop), 0)));

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Test Barbershop');
    });

    const editButton = screen.getByRole('button', { name: /editar/i });
    await userEvent.click(editButton);

    expect(mockNavigate).toHaveBeenCalledWith('/barbearias/1/editar');
  });

  it('navigates to list when voltar à lista button is clicked from not found state', async () => {
    mockGetById.mockReturnValue(new Promise(resolve => setTimeout(() => resolve(null as any), 0)));

    render(
      <MemoryRouter initialEntries={['/barbearias/1']}>
        <Routes>
          <Route path="/barbearias/:id" element={<BarbershopDetails />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Barbearia não encontrada')).toBeInTheDocument();
    });

    const backToListButton = screen.getByRole('button', { name: /voltar à lista/i });
    await userEvent.click(backToListButton);

    expect(mockNavigate).toHaveBeenCalledWith('/barbearias');
  });
});