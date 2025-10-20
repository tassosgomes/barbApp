import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useAppointmentActions } from '../useAppointmentActions';

// Mock the appointments service
vi.mock('@/services/appointments.service', () => ({
  appointmentsService: {
    confirm: vi.fn(),
    cancel: vi.fn(),
    complete: vi.fn(),
  },
  AppointmentError: class AppointmentError extends Error {
    constructor(message: string, public statusCode: number) {
      super(message);
      this.name = 'AppointmentError';
    }
  },
}));

import { appointmentsService } from '@/services/appointments.service';

const mockAppointmentsService = vi.mocked(appointmentsService);

describe('useAppointmentActions Hook', () => {
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

  describe('confirm action', () => {
    it('should confirm appointment successfully', async () => {
      const mockId = 'apt-123';
      mockAppointmentsService.confirm.mockResolvedValue(undefined);

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      expect(result.current.isLoading).toBe(false);

      act(() => {
        result.current.confirm(mockId);
      });

      // Wait for the mutation to complete
      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockAppointmentsService.confirm).toHaveBeenCalledWith(mockId);
    });

    it('should call onSuccess callback on successful confirm', async () => {
      const mockId = 'apt-123';
      const onSuccess = vi.fn();
      mockAppointmentsService.confirm.mockResolvedValue(undefined);

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.confirm(mockId, { onSuccess });
      });

      await waitFor(() => {
        expect(onSuccess).toHaveBeenCalled();
      });
    });

    it('should call onError callback on failed confirm', async () => {
      const mockId = 'apt-123';
      const error = new Error('Failed to confirm');
      const onError = vi.fn();
      mockAppointmentsService.confirm.mockRejectedValue(error);

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.confirm(mockId, { onError });
      });

      await waitFor(() => {
        expect(onError).toHaveBeenCalled();
      });
      
      // Verify it was called with the error (first argument)
      expect(onError.mock.calls[0][0]).toBe(error);
    });

    it('should invalidate queries after successful confirm', async () => {
      const mockId = 'apt-123';
      mockAppointmentsService.confirm.mockResolvedValue(undefined);

      // Pre-populate cache with schedule data
      queryClient.setQueryData(['barber-schedule', '2025-10-20'], { appointments: [] });

      const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.confirm(mockId);
      });

      await waitFor(() => {
        expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['barber-schedule'] });
        expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['appointment-details'] });
      });
    });
  });

  describe('cancel action', () => {
    it('should cancel appointment successfully', async () => {
      const mockId = 'apt-123';
      mockAppointmentsService.cancel.mockResolvedValue(undefined);

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.cancel(mockId);
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockAppointmentsService.cancel).toHaveBeenCalledWith(mockId);
    });

    it('should call onSuccess callback on successful cancel', async () => {
      const mockId = 'apt-123';
      const onSuccess = vi.fn();
      mockAppointmentsService.cancel.mockResolvedValue(undefined);

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.cancel(mockId, { onSuccess });
      });

      await waitFor(() => {
        expect(onSuccess).toHaveBeenCalled();
      });
    });

    it('should invalidate queries after successful cancel', async () => {
      const mockId = 'apt-123';
      mockAppointmentsService.cancel.mockResolvedValue(undefined);

      const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.cancel(mockId);
      });

      await waitFor(() => {
        expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['barber-schedule'] });
        expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['appointment-details'] });
      });
    });
  });

  describe('complete action', () => {
    it('should complete appointment successfully', async () => {
      const mockId = 'apt-123';
      mockAppointmentsService.complete.mockResolvedValue(undefined);

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.complete(mockId);
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockAppointmentsService.complete).toHaveBeenCalledWith(mockId);
    });

    it('should call onSuccess callback on successful complete', async () => {
      const mockId = 'apt-123';
      const onSuccess = vi.fn();
      mockAppointmentsService.complete.mockResolvedValue(undefined);

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.complete(mockId, { onSuccess });
      });

      await waitFor(() => {
        expect(onSuccess).toHaveBeenCalled();
      });
    });

    it('should invalidate queries after successful complete', async () => {
      const mockId = 'apt-123';
      mockAppointmentsService.complete.mockResolvedValue(undefined);

      const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.complete(mockId);
      });

      await waitFor(() => {
        expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['barber-schedule'] });
        expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['appointment-details'] });
      });
    });
  });

  describe('loading states', () => {
    it('should expose individual mutation loading states', async () => {
      mockAppointmentsService.confirm.mockImplementation(
        () => new Promise(resolve => setTimeout(resolve, 100))
      );

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.confirm('apt-123');
      });

      // Wait for pending state to be true
      await waitFor(() => {
        expect(result.current.confirmState.isPending).toBe(true);
      });
      
      expect(result.current.cancelState.isPending).toBe(false);
      expect(result.current.completeState.isPending).toBe(false);

      await waitFor(() => {
        expect(result.current.confirmState.isPending).toBe(false);
      });
    });

    it('should set global isLoading when any mutation is pending', async () => {
      mockAppointmentsService.cancel.mockImplementation(
        () => new Promise(resolve => setTimeout(resolve, 100))
      );

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.cancel('apt-123');
      });

      // Wait for loading state to be true
      await waitFor(() => {
        expect(result.current.isLoading).toBe(true);
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });
    });
  });

  describe('error handling', () => {
    it('should expose error from mutations', async () => {
      const error = new Error('Failed to confirm');
      mockAppointmentsService.confirm.mockRejectedValue(error);

      const { result } = renderHook(() => useAppointmentActions(), { wrapper });

      act(() => {
        result.current.confirm('apt-123');
      });

      await waitFor(() => {
        expect(result.current.error).toEqual(error);
        expect(result.current.confirmState.error).toEqual(error);
      });
    });
  });
});
