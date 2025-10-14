import { renderHook, waitFor } from '@testing-library/react';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { useBarbershop } from '@/hooks/useBarbershop';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop } from '@/types';

// Mock the service
vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    getById: vi.fn(),
  },
}));

const mockBarbershopService = vi.mocked(barbershopService);

describe('useBarbershop', () => {
  const mockBarbershop: Barbershop = {
    id: '1',
    name: 'Barbearia Test',
    document: '12345678901234',
    ownerName: 'João Silva',
    email: 'joao@test.com',
    phone: '(11) 99999-9999',
    code: 'ABC12345',
    address: {
      street: 'Rua Teste',
      number: '123',
      neighborhood: 'Centro',
      city: 'São Paulo',
      state: 'SP',
      zipCode: '01234-567',
    },
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should initialize with loading state', () => {
    mockBarbershopService.getById.mockImplementation(() => new Promise(() => {})); // Never resolves

    const { result } = renderHook(() => useBarbershop('1'));

    expect(result.current.loading).toBe(true);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it('should fetch barbershop data successfully', async () => {
    mockBarbershopService.getById.mockResolvedValue(mockBarbershop);

    const { result } = renderHook(() => useBarbershop('1'));

    // Initially loading
    expect(result.current.loading).toBe(true);
    expect(result.current.data).toBeNull();

    // Wait for data to load
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data).toEqual(mockBarbershop);
    expect(result.current.error).toBeNull();
    expect(mockBarbershopService.getById).toHaveBeenCalledWith('1');
    expect(mockBarbershopService.getById).toHaveBeenCalledTimes(1);
  });

  it('should handle error state', async () => {
    const mockError = new Error('Failed to fetch barbershop');
    mockBarbershopService.getById.mockRejectedValue(mockError);

    const { result } = renderHook(() => useBarbershop('1'));

    // Initially loading
    expect(result.current.loading).toBe(true);

    // Wait for error
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data).toBeNull();
    expect(result.current.error).toEqual(mockError);
    expect(mockBarbershopService.getById).toHaveBeenCalledWith('1');
  });

  it('should not fetch data when id is undefined', () => {
    const { result } = renderHook(() => useBarbershop(undefined));

    expect(result.current.loading).toBe(true);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();
    expect(mockBarbershopService.getById).not.toHaveBeenCalled();
  });

  it('should not fetch data when id is empty string', () => {
    const { result } = renderHook(() => useBarbershop(''));

    expect(result.current.loading).toBe(true);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();
    expect(mockBarbershopService.getById).not.toHaveBeenCalled();
  });

  it('should refetch when id changes', async () => {
    mockBarbershopService.getById.mockResolvedValue(mockBarbershop);

    const { result, rerender } = renderHook(
      ({ id }) => useBarbershop(id),
      { initialProps: { id: '1' } }
    );

    // Wait for initial load
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(mockBarbershopService.getById).toHaveBeenCalledTimes(1);
    expect(mockBarbershopService.getById).toHaveBeenCalledWith('1');

    // Change id
    const newBarbershop = { ...mockBarbershop, id: '2', name: 'Barbearia Nova' };
    mockBarbershopService.getById.mockResolvedValue(newBarbershop);

    rerender({ id: '2' });

    // Wait for refetch
    await waitFor(() => {
      expect(result.current.data?.id).toBe('2');
    });

    expect(mockBarbershopService.getById).toHaveBeenCalledTimes(2);
    expect(mockBarbershopService.getById).toHaveBeenLastCalledWith('2');
  });

  it('should handle network errors gracefully', async () => {
    const networkError = new Error('Network Error: Failed to fetch');
    mockBarbershopService.getById.mockRejectedValue(networkError);

    const { result } = renderHook(() => useBarbershop('1'));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data).toBeNull();
    expect(result.current.error).toEqual(networkError);
  });
});