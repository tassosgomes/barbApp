import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { useBarbers } from '@/hooks/useBarbers';
import { barbersService } from '@/services/barbers.service';
import type { BarberFilters, PaginatedResponse, Barber } from '@/types';

// Mock the service
vi.mock('@/services/barbers.service', () => ({
  barbersService: {
    list: vi.fn(),
  },
}));

const mockBarbersService = vi.mocked(barbersService);

// Create a test QueryClient
const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  });

// Test wrapper component
function TestWrapper({ children }: { children: React.ReactNode }) {
  return (
    <QueryClientProvider client={createTestQueryClient()}>
      {children}
    </QueryClientProvider>
  );
}

describe('useBarbers', () => {
  const mockFilters: BarberFilters = {
    page: 1,
    pageSize: 10,
    searchName: 'João',
    isActive: true,
  };

  const mockBarber: Barber = {
    id: '1',
    name: 'João Silva',
    email: 'joao@test.com',
    phoneFormatted: '(11) 99999-9999',
    services: [
      {
        id: '1',
        name: 'Corte de Cabelo',
      },
    ],
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  };

  const mockResponse: PaginatedResponse<Barber> = {
    items: [mockBarber],
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

  it('should return loading state initially', () => {
    mockBarbersService.list.mockImplementation(() => new Promise(() => {})); // Never resolves

    const { result } = renderHook(() => useBarbers(mockFilters), {
      wrapper: TestWrapper,
    });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.data).toBeUndefined();
    expect(result.current.error).toBeNull();
  });

  it('should fetch barbers successfully', async () => {
    mockBarbersService.list.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useBarbers(mockFilters), {
      wrapper: TestWrapper,
    });

    // Initially loading
    expect(result.current.isLoading).toBe(true);

    // Wait for data to load
    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockResponse);
    expect(result.current.error).toBeNull();
    expect(mockBarbersService.list).toHaveBeenCalledWith(mockFilters);
  });

  it('should handle error state', async () => {
    const mockError = new Error('Failed to fetch barbers');
    mockBarbersService.list.mockRejectedValue(mockError);

    const { result } = renderHook(() => useBarbers(mockFilters), {
      wrapper: TestWrapper,
    });

    // Initially loading
    expect(result.current.isLoading).toBe(true);

    // Wait for error
    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.error).toEqual(mockError);
  });

  it('should keep previous data when filters change', async () => {
    mockBarbersService.list.mockResolvedValue(mockResponse);

    const { result, rerender } = renderHook(
      ({ filters }) => useBarbers(filters),
      {
        wrapper: TestWrapper,
        initialProps: { filters: mockFilters },
      }
    );

    // Wait for initial data
    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockResponse);

    // Change filters - should keep previous data
    const newFilters = { ...mockFilters, searchName: 'Maria' };
    mockBarbersService.list.mockResolvedValueOnce({
      ...mockResponse,
      items: [{ ...mockBarber, name: 'Maria Santos' }],
    });
    rerender({ filters: newFilters });

    // Should still have previous data while loading new data
    expect(result.current.data).toEqual(mockResponse);
    expect(result.current.isPlaceholderData).toBe(true);

    // Wait for new data
    await waitFor(() => {
      expect(result.current.isPlaceholderData).toBe(false);
    });
  });
});