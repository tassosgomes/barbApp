import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { useBarberMutations } from '@/hooks/useBarberMutations';
import { barbersService } from '@/services/barbers.service';
import type { CreateBarberRequest, UpdateBarberRequest } from '@/types';

// Mock the service
vi.mock('@/services/barbers.service', () => ({
  barbersService: {
    create: vi.fn(),
    update: vi.fn(),
    toggleActive: vi.fn(),
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

describe('useBarberMutations', () => {
  const mockCreateRequest: CreateBarberRequest = {
    name: 'João Silva',
    email: 'joao@test.com',
    password: 'password123',
    phone: '(11) 99999-9999',
    serviceIds: ['1'],
  };

  const mockUpdateRequest: UpdateBarberRequest = {
    name: 'João Silva Atualizado',
    phone: '(11) 88888-8888',
    serviceIds: ['1', '2'],
  };

  const mockCreatedBarber = {
    id: '1',
    name: 'João Silva',
    email: 'joao@test.com',
    phoneFormatted: '(11) 99999-9999',
    services: [{ id: '1', name: 'Corte de Cabelo' }],
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  };

  const mockUpdatedBarber = {
    id: '1',
    name: 'João Silva Atualizado',
    email: 'joao@test.com',
    phoneFormatted: '(11) 88888-8888',
    services: [
      { id: '1', name: 'Corte de Cabelo' },
      { id: '2', name: 'Barba' },
    ],
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('createBarber', () => {
    it('should create barber successfully', async () => {
      mockBarbersService.create.mockResolvedValue(mockCreatedBarber);

      const { result } = renderHook(() => useBarberMutations(), {
        wrapper: TestWrapper,
      });

      result.current.createBarber.mutate(mockCreateRequest);

      await waitFor(() => {
        expect(result.current.createBarber.isSuccess).toBe(true);
      });

      expect(result.current.createBarber.data).toEqual(mockCreatedBarber);
      expect(result.current.createBarber.error).toBeNull();
      expect(mockBarbersService.create).toHaveBeenCalledWith(mockCreateRequest);
    });

    it('should handle create error', async () => {
      const mockError = new Error('Failed to create barber');
      mockBarbersService.create.mockRejectedValue(mockError);

      const { result } = renderHook(() => useBarberMutations(), {
        wrapper: TestWrapper,
      });

      result.current.createBarber.mutate(mockCreateRequest);

      await waitFor(() => {
        expect(result.current.createBarber.isError).toBe(true);
      });

      expect(result.current.createBarber.data).toBeUndefined();
      expect(result.current.createBarber.error).toEqual(mockError);
    });
  });

  describe('updateBarber', () => {
    it('should update barber successfully', async () => {
      mockBarbersService.update.mockResolvedValue(mockUpdatedBarber);

      const { result } = renderHook(() => useBarberMutations(), {
        wrapper: TestWrapper,
      });

      result.current.updateBarber.mutate({ id: '1', request: mockUpdateRequest });

      await waitFor(() => {
        expect(result.current.updateBarber.isSuccess).toBe(true);
      });

      expect(result.current.updateBarber.data).toEqual(mockUpdatedBarber);
      expect(result.current.updateBarber.error).toBeNull();
      expect(mockBarbersService.update).toHaveBeenCalledWith('1', mockUpdateRequest);
    });

    it('should handle update error', async () => {
      const mockError = new Error('Failed to update barber');
      mockBarbersService.update.mockRejectedValue(mockError);

      const { result } = renderHook(() => useBarberMutations(), {
        wrapper: TestWrapper,
      });

      result.current.updateBarber.mutate({ id: '1', request: mockUpdateRequest });

      await waitFor(() => {
        expect(result.current.updateBarber.isError).toBe(true);
      });

      expect(result.current.updateBarber.data).toBeUndefined();
      expect(result.current.updateBarber.error).toEqual(mockError);
    });
  });

  describe('toggleBarberActive', () => {
    it('should deactivate barber successfully', async () => {
      mockBarbersService.toggleActive.mockResolvedValue(undefined);

      const { result } = renderHook(() => useBarberMutations(), {
        wrapper: TestWrapper,
      });

      result.current.toggleBarberActive.mutate({ id: '1', isActive: false });

      await waitFor(() => {
        expect(result.current.toggleBarberActive.isSuccess).toBe(true);
      });

      expect(result.current.toggleBarberActive.data).toBeUndefined();
      expect(result.current.toggleBarberActive.error).toBeNull();
      expect(mockBarbersService.toggleActive).toHaveBeenCalledWith('1', false);
    });

    it('should handle toggle error', async () => {
      const mockError = new Error('Failed to toggle barber status');
      mockBarbersService.toggleActive.mockRejectedValue(mockError);

      const { result } = renderHook(() => useBarberMutations(), {
        wrapper: TestWrapper,
      });

      result.current.toggleBarberActive.mutate({ id: '1', isActive: false });

      await waitFor(() => {
        expect(result.current.toggleBarberActive.isError).toBe(true);
      });

      expect(result.current.toggleBarberActive.data).toBeUndefined();
      expect(result.current.toggleBarberActive.error).toEqual(mockError);
    });
  });
});