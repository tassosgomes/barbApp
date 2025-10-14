import { renderHook, waitFor } from '@testing-library/react';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { useBarbershops } from '@/hooks/useBarbershops';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop, BarbershopFilters, PaginatedResponse } from '@/types';

// Mock the service
vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    getAll: vi.fn(),
  },
}));

const mockBarbershopService = vi.mocked(barbershopService);

describe('useBarbershops', () => {
  const mockFilters: BarbershopFilters = {
    pageNumber: 1,
    pageSize: 10,
    searchTerm: 'test',
    isActive: true,
  };

  const mockResponse: PaginatedResponse<Barbershop> = {
    items: [
      {
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
      },
    ],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 1,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should initialize with loading state', () => {
    mockBarbershopService.getAll.mockImplementation(() => new Promise(() => {})); // Never resolves

    const { result } = renderHook(() => useBarbershops(mockFilters));

    expect(result.current.loading).toBe(true);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it('should fetch data successfully', async () => {
    mockBarbershopService.getAll.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useBarbershops(mockFilters));

    // Initially loading
    expect(result.current.loading).toBe(true);
    expect(result.current.data).toBeNull();

    // Wait for data to load
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data).toEqual(mockResponse);
    expect(result.current.error).toBeNull();
    expect(mockBarbershopService.getAll).toHaveBeenCalledWith(mockFilters);
    expect(mockBarbershopService.getAll).toHaveBeenCalledTimes(1);
  });

  it('should handle error state', async () => {
    const mockError = new Error('Failed to fetch barbershops');
    mockBarbershopService.getAll.mockRejectedValue(mockError);

    const { result } = renderHook(() => useBarbershops(mockFilters));

    // Initially loading
    expect(result.current.loading).toBe(true);

    // Wait for error
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data).toBeNull();
    expect(result.current.error).toEqual(mockError);
    expect(mockBarbershopService.getAll).toHaveBeenCalledWith(mockFilters);
  });

  it('should refetch data when refetch is called', async () => {
    mockBarbershopService.getAll.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useBarbershops(mockFilters));

    // Wait for initial load
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(mockBarbershopService.getAll).toHaveBeenCalledTimes(1);

    // Call refetch
    await result.current.refetch();

    expect(mockBarbershopService.getAll).toHaveBeenCalledTimes(2);
    expect(mockBarbershopService.getAll).toHaveBeenCalledWith(mockFilters);
  });

  it('should refetch when filters change', async () => {
    mockBarbershopService.getAll.mockResolvedValue(mockResponse);

    let filters = { ...mockFilters };
    const { result, rerender } = renderHook(() => useBarbershops(filters));

    // Wait for initial load
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(mockBarbershopService.getAll).toHaveBeenCalledTimes(1);
    expect(mockBarbershopService.getAll).toHaveBeenCalledWith(filters);

    // Change filters
    filters = { ...filters, searchTerm: 'new search' };
    rerender();

    // Wait for refetch
    await waitFor(() => {
      expect(mockBarbershopService.getAll).toHaveBeenCalledTimes(2);
    });

    expect(mockBarbershopService.getAll).toHaveBeenLastCalledWith(filters);
  });

  it('should reset error when refetching', async () => {
    const mockError = new Error('Network error');
    mockBarbershopService.getAll
      .mockRejectedValueOnce(mockError)
      .mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useBarbershops(mockFilters));

    // Wait for error
    await waitFor(() => {
      expect(result.current.error).toEqual(mockError);
    });

    // Call refetch
    result.current.refetch();

    // Wait for error to be cleared and data loaded
    await waitFor(() => {
      expect(result.current.error).toBeNull();
      expect(result.current.data).toEqual(mockResponse);
    });
  });

  it('should handle empty filters', async () => {
    const emptyFilters = {};
    mockBarbershopService.getAll.mockResolvedValue({
      ...mockResponse,
      items: [],
      totalCount: 0,
    });

    const { result } = renderHook(() => useBarbershops(emptyFilters));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data?.items).toEqual([]);
    expect(result.current.data?.totalCount).toBe(0);
    expect(mockBarbershopService.getAll).toHaveBeenCalledWith(emptyFilters);
  });
});