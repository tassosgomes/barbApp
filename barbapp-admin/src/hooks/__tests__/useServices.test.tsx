import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useServices } from '../useServices';
import type { ServiceFilters } from '@/types';

// Mock the services service
vi.mock('@/services/services.service', () => ({
  servicesService: {
    list: vi.fn(),
  },
}));

import { servicesService } from '@/services/services.service';

const mockServicesService = vi.mocked(servicesService);

describe('useServices Hook', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  it('should fetch services with default filters', async () => {
    const mockResponse = {
      items: [
        {
          id: '1',
          name: 'Corte de Cabelo',
          description: 'Corte masculino completo',
          durationMinutes: 30,
          price: 25.00,
          isActive: true,
        },
      ],
      pageNumber: 1,
      pageSize: 20,
      totalPages: 1,
      totalCount: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };

    mockServicesService.list.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useServices({}), { wrapper });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.data).toEqual(mockResponse);
    });

    expect(mockServicesService.list).toHaveBeenCalledWith({});
  });

  it('should fetch services with filters', async () => {
    const filters: ServiceFilters = {
      searchName: 'Corte',
      isActive: true,
      page: 1,
      pageSize: 10,
    };

    const mockResponse = {
      items: [
        {
          id: '1',
          name: 'Corte de Cabelo',
          description: 'Corte masculino completo',
          durationMinutes: 30,
          price: 25.00,
          isActive: true,
        },
      ],
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      totalCount: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };

    mockServicesService.list.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useServices(filters), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.data).toEqual(mockResponse);
    });

    expect(mockServicesService.list).toHaveBeenCalledWith(filters);
  });

  it('should handle error state', async () => {
    const error = new Error('Failed to fetch services');
    mockServicesService.list.mockRejectedValue(error);

    const { result } = renderHook(() => useServices({}), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toEqual(error);
    });
  });

  it('should use correct query key', async () => {
    const filters: ServiceFilters = {
      searchName: 'Barba',
      isActive: false,
    };

    mockServicesService.list.mockResolvedValue({
      items: [],
      pageNumber: 1,
      pageSize: 20,
      totalPages: 0,
      totalCount: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    });

    renderHook(() => useServices(filters), { wrapper });

    await waitFor(() => {
      expect(mockServicesService.list).toHaveBeenCalledWith(filters);
    });

    // The query key should include the filters
    expect(mockServicesService.list).toHaveBeenCalledTimes(1);
  });

  it('should keep previous data while loading new data', async () => {
    const initialResponse = {
      items: [
        {
          id: '1',
          name: 'Corte de Cabelo',
          description: 'Corte masculino completo',
          durationMinutes: 30,
          price: 25.00,
          isActive: true,
        },
      ],
      pageNumber: 1,
      pageSize: 20,
      totalPages: 1,
      totalCount: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };

    const newResponse = {
      items: [
        {
          id: '2',
          name: 'Barba',
          description: 'Aparação da barba',
          durationMinutes: 20,
          price: 15.00,
          isActive: true,
        },
      ],
      pageNumber: 2,
      pageSize: 20,
      totalPages: 1,
      totalCount: 1,
      hasPreviousPage: true,
      hasNextPage: false,
    };

    mockServicesService.list.mockResolvedValueOnce(initialResponse);
    mockServicesService.list.mockResolvedValueOnce(newResponse);

    const { result, rerender } = renderHook(
      ({ filters }: { filters: ServiceFilters }) => useServices(filters),
      {
        wrapper,
        initialProps: { filters: { page: 1 } },
      }
    );

    // Wait for initial load
    await waitFor(() => {
      expect(result.current.data).toEqual(initialResponse);
    });

    // Change filters to trigger new query
    rerender({ filters: { page: 2 } });

    // Should keep previous data while loading
    expect(result.current.data).toEqual(initialResponse);

    // Wait for new data
    await waitFor(() => {
      expect(result.current.data).toEqual(newResponse);
    });
  });
});