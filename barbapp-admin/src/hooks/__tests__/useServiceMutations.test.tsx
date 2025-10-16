import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useServiceMutations } from '../useServiceMutations';
import type { CreateServiceRequest, UpdateServiceRequest } from '@/types';

// Mock the services service
vi.mock('@/services/services.service', () => ({
  servicesService: {
    create: vi.fn(),
    update: vi.fn(),
    toggleActive: vi.fn(),
  },
}));

import { servicesService } from '@/services/services.service';

const mockServicesService = vi.mocked(servicesService);

describe('useServiceMutations Hook', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
        mutations: {
          retry: false,
        },
      },
    });
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  describe('createService', () => {
    it('should create service successfully', async () => {
      const request: CreateServiceRequest = {
        name: 'Novo Serviço',
        description: 'Descrição do serviço',
        durationMinutes: 45,
        price: 35.00,
      };

      const mockResponse = {
        id: '1',
        ...request,
        isActive: true,
      };

      mockServicesService.create.mockResolvedValue(mockResponse);

      const { result } = renderHook(() => useServiceMutations(), { wrapper });

      result.current.createService.mutate(request);

      await waitFor(() => {
        expect(result.current.createService.isSuccess).toBe(true);
        expect(result.current.createService.data).toEqual(mockResponse);
      });

      expect(mockServicesService.create).toHaveBeenCalledWith(request);
    });

    it('should invalidate services queries on success', async () => {
      const request: CreateServiceRequest = {
        name: 'Novo Serviço',
        description: 'Descrição',
        durationMinutes: 30,
        price: 25.00,
      };

      const mockResponse = {
        id: '1',
        ...request,
        isActive: true,
      };

      mockServicesService.create.mockResolvedValue(mockResponse);

      // Set up initial query data
      queryClient.setQueryData(['services', {}], {
        items: [],
        pageNumber: 1,
        pageSize: 20,
        totalPages: 0,
        totalCount: 0,
        hasPreviousPage: false,
        hasNextPage: false,
      });

      const { result } = renderHook(() => useServiceMutations(), { wrapper });

      result.current.createService.mutate(request);

      await waitFor(() => {
        expect(result.current.createService.isSuccess).toBe(true);
      });

      // Check if queries were invalidated
      expect(queryClient.getQueryState(['services', {}])?.isInvalidated).toBe(true);
    });

    it('should handle create error', async () => {
      const request: CreateServiceRequest = {
        name: 'Novo Serviço',
        description: 'Descrição',
        durationMinutes: 30,
        price: 25.00,
      };

      const error = new Error('Failed to create service');
      mockServicesService.create.mockRejectedValue(error);

      const { result } = renderHook(() => useServiceMutations(), { wrapper });

      result.current.createService.mutate(request);

      await waitFor(() => {
        expect(result.current.createService.isError).toBe(true);
        expect(result.current.createService.error).toEqual(error);
      });
    });
  });

  describe('updateService', () => {
    it('should update service successfully', async () => {
      const request: UpdateServiceRequest = {
        name: 'Serviço Atualizado',
        description: 'Descrição atualizada',
        durationMinutes: 60,
        price: 40.00,
      };

      const mockResponse = {
        id: '1',
        ...request,
        isActive: true,
      };

      mockServicesService.update.mockResolvedValue(mockResponse);

      const { result } = renderHook(() => useServiceMutations(), { wrapper });

      result.current.updateService.mutate({ id: '1', request });

      await waitFor(() => {
        expect(result.current.updateService.isSuccess).toBe(true);
        expect(result.current.updateService.data).toEqual(mockResponse);
      });

      expect(mockServicesService.update).toHaveBeenCalledWith('1', request);
    });

    it('should invalidate services queries on update success', async () => {
      const request: UpdateServiceRequest = {
        name: 'Atualizado',
        description: 'Desc',
        durationMinutes: 30,
        price: 25.00,
      };

      mockServicesService.update.mockResolvedValue({
        id: '1',
        ...request,
        isActive: true,
      });

      queryClient.setQueryData(['services', {}], {
        items: [{ id: '1', name: 'Old Name', description: 'Old', durationMinutes: 20, price: 20, isActive: true }],
        pageNumber: 1,
        pageSize: 20,
        totalPages: 1,
        totalCount: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      });

      const { result } = renderHook(() => useServiceMutations(), { wrapper });

      result.current.updateService.mutate({ id: '1', request });

      await waitFor(() => {
        expect(result.current.updateService.isSuccess).toBe(true);
      });

      expect(queryClient.getQueryState(['services', {}])?.isInvalidated).toBe(true);
    });
  });

  describe('toggleServiceActive', () => {
    it('should deactivate service successfully', async () => {
      mockServicesService.toggleActive.mockResolvedValue(undefined);

      const { result } = renderHook(() => useServiceMutations(), { wrapper });

      result.current.toggleServiceActive.mutate({ id: '1', isActive: false });

      await waitFor(() => {
        expect(result.current.toggleServiceActive.isSuccess).toBe(true);
      });

      expect(mockServicesService.toggleActive).toHaveBeenCalledWith('1', false);
    });

    it('should invalidate services queries on toggle success', async () => {
      mockServicesService.toggleActive.mockResolvedValue(undefined);

      queryClient.setQueryData(['services', {}], {
        items: [{ id: '1', name: 'Service', description: 'Desc', durationMinutes: 30, price: 25, isActive: true }],
        pageNumber: 1,
        pageSize: 20,
        totalPages: 1,
        totalCount: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      });

      const { result } = renderHook(() => useServiceMutations(), { wrapper });

      result.current.toggleServiceActive.mutate({ id: '1', isActive: false });

      await waitFor(() => {
        expect(result.current.toggleServiceActive.isSuccess).toBe(true);
      });

      expect(queryClient.getQueryState(['services', {}])?.isInvalidated).toBe(true);
    });

    it('should throw error when trying to activate service', async () => {
      const error = new Error('Activation endpoint not implemented yet');
      mockServicesService.toggleActive.mockRejectedValue(error);

      const { result } = renderHook(() => useServiceMutations(), { wrapper });

      result.current.toggleServiceActive.mutate({ id: '1', isActive: true });

      await waitFor(() => {
        expect(result.current.toggleServiceActive.isError).toBe(true);
        expect(result.current.toggleServiceActive.error).toEqual(error);
      });

      expect(mockServicesService.toggleActive).toHaveBeenCalledWith('1', true);
    });
  });
});