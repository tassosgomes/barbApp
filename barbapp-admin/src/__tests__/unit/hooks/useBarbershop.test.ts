import { renderHook, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useBarbershop } from '@/hooks/useBarbershop';
import { barbershopService } from '@/services/barbershop.service';

// Mock the service
vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    getById: vi.fn(),
  },
}));

const mockGetById = vi.mocked(barbershopService.getById);

describe('useBarbershop', () => {
  const mockBarbershop = {
    id: '1',
    name: 'Barbearia Teste',
    document: '12.345.678/0001-90',
    phone: '(11) 99999-9999',
    ownerName: 'João Silva',
    email: 'teste@email.com',
    code: 'ABC123XY',
    isActive: true,
    address: {
      zipCode: '01000-000',
      street: 'Rua Teste',
      number: '123',
      neighborhood: 'Centro',
      city: 'São Paulo',
      state: 'SP',
    },
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should return null data when id is undefined', () => {
    const { result } = renderHook(() => useBarbershop(undefined));

    expect(result.current.data).toBe(null);
    expect(result.current.loading).toBe(true);
    expect(result.current.error).toBe(null);
    expect(mockGetById).not.toHaveBeenCalled();
  });

  it('should fetch barbershop data successfully', async () => {
    mockGetById.mockResolvedValue(mockBarbershop);

    const { result } = renderHook(() => useBarbershop('1'));

    expect(result.current.loading).toBe(true);

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data).toEqual(mockBarbershop);
    expect(result.current.error).toBe(null);
    expect(mockGetById).toHaveBeenCalledWith('1');
  });

  it('should handle error state', async () => {
    const mockError = new Error('Barbershop not found');
    mockGetById.mockRejectedValue(mockError);

    const { result } = renderHook(() => useBarbershop('1'));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.error).toEqual(mockError);
    expect(result.current.data).toBe(null);
  });

  it('should refetch when id changes', async () => {
    mockGetById.mockResolvedValue(mockBarbershop);

    const { result, rerender } = renderHook(
      (id) => useBarbershop(id),
      { initialProps: '1' }
    );

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    // Change id
    const newBarbershop = { ...mockBarbershop, id: '2', name: 'Nova Barbearia' };
    mockGetById.mockResolvedValue(newBarbershop);
    rerender('2');

    await waitFor(() => {
      expect(result.current.data?.id).toBe('2');
    });

    expect(mockGetById).toHaveBeenCalledTimes(2);
    expect(mockGetById).toHaveBeenLastCalledWith('2');
  });
});