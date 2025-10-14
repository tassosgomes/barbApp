import { renderHook, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useBarbershops } from '@/hooks/useBarbershops';
import { barbershopService } from '@/services/barbershop.service';

// Mock the service
vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    getAll: vi.fn(),
  },
}));

const mockGetAll = vi.mocked(barbershopService.getAll);

describe('useBarbershops', () => {
  const mockFilters = {
    pageNumber: 1,
    pageSize: 20,
    searchTerm: 'test',
    isActive: true,
  };

  const mockResponse = {
    items: [
      {
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
      },
    ],
    pageNumber: 1,
    pageSize: 20,
    totalPages: 1,
    totalCount: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should return initial loading state', () => {
    mockGetAll.mockImplementation(() => new Promise(() => {})); // Never resolves

    const { result } = renderHook(() => useBarbershops(mockFilters));

    expect(result.current.loading).toBe(true);
    expect(result.current.error).toBe(null);
    expect(result.current.data).toBe(null);
  });

  it('should fetch data successfully', async () => {
    mockGetAll.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useBarbershops(mockFilters));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data).toEqual(mockResponse);
    expect(result.current.error).toBe(null);
    expect(mockGetAll).toHaveBeenCalledWith(mockFilters);
  });

  it('should handle error state', async () => {
    const mockError = new Error('API Error');
    mockGetAll.mockRejectedValue(mockError);

    const { result } = renderHook(() => useBarbershops(mockFilters));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.error).toEqual(mockError);
    expect(result.current.data).toBe(null);
  });

  it('should refetch when filters change', async () => {
    mockGetAll.mockResolvedValue(mockResponse);

    const { result, rerender } = renderHook(
      (filters) => useBarbershops(filters),
      { initialProps: mockFilters }
    );

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    // Change filters
    const newFilters = { ...mockFilters, searchTerm: 'new search' };
    rerender(newFilters);

    expect(result.current.loading).toBe(true);
    expect(mockGetAll).toHaveBeenCalledTimes(2);
    expect(mockGetAll).toHaveBeenLastCalledWith(newFilters);
  });

  it('should provide refetch function', async () => {
    mockGetAll.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useBarbershops(mockFilters));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    // Call refetch
    result.current.refetch();

    expect(mockGetAll).toHaveBeenCalledTimes(2);
  });
});