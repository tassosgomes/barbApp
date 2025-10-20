import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useAppointmentDetails } from '../useAppointmentDetails';
import { AppointmentStatus } from '@/types/appointment';

// Mock the appointments service
vi.mock('@/services/appointments.service', () => ({
  appointmentsService: {
    getDetails: vi.fn(),
  },
}));

import { appointmentsService } from '@/services/appointments.service';

const mockAppointmentsService = vi.mocked(appointmentsService);

describe('useAppointmentDetails Hook', () => {
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

  it('should fetch appointment details when id is provided', async () => {
    const mockId = 'apt-123';
    const mockResponse = {
      id: mockId,
      customerName: 'Cliente 1',
      customerPhone: '11999999999',
      serviceTitle: 'Corte de Cabelo',
      servicePrice: 50,
      serviceDurationMinutes: 30,
      startTime: '2025-10-20T09:00:00Z',
      endTime: '2025-10-20T09:30:00Z',
      status: AppointmentStatus.Confirmed,
      createdAt: '2025-10-19T10:00:00Z',
      confirmedAt: '2025-10-19T11:00:00Z',
    };

    mockAppointmentsService.getDetails.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useAppointmentDetails(mockId), { wrapper });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.data).toEqual(mockResponse);
    });

    expect(mockAppointmentsService.getDetails).toHaveBeenCalledWith(mockId);
  });

  it('should not fetch when id is null', async () => {
    const { result } = renderHook(() => useAppointmentDetails(null), { wrapper });

    // Query should be disabled
    expect(result.current.isLoading).toBe(false);
    expect(result.current.data).toBeUndefined();
    expect(mockAppointmentsService.getDetails).not.toHaveBeenCalled();

    // Wait to ensure no fetch happens
    await new Promise(resolve => setTimeout(resolve, 100));
    expect(mockAppointmentsService.getDetails).not.toHaveBeenCalled();
  });

  it('should handle error state', async () => {
    const mockId = 'apt-123';
    const error = new Error('Appointment not found');
    mockAppointmentsService.getDetails.mockRejectedValue(error);

    const { result } = renderHook(() => useAppointmentDetails(mockId), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toEqual(error);
    });
  });

  it('should enable query when id changes from null to value', async () => {
    const mockId = 'apt-123';
    const mockResponse = {
      id: mockId,
      customerName: 'Cliente 1',
      customerPhone: '11999999999',
      serviceTitle: 'Corte',
      servicePrice: 50,
      serviceDurationMinutes: 30,
      startTime: '2025-10-20T09:00:00Z',
      endTime: '2025-10-20T09:30:00Z',
      status: AppointmentStatus.Pending,
      createdAt: '2025-10-19T10:00:00Z',
    };

    mockAppointmentsService.getDetails.mockResolvedValue(mockResponse);

    const { rerender, result } = renderHook(
      ({ id }) => useAppointmentDetails(id),
      {
        wrapper,
        initialProps: { id: null as string | null },
      }
    );

    // Initially no fetch
    expect(mockAppointmentsService.getDetails).not.toHaveBeenCalled();

    // Change id to a value
    rerender({ id: mockId });

    await waitFor(() => {
      expect(mockAppointmentsService.getDetails).toHaveBeenCalledWith(mockId);
      expect(result.current.data).toEqual(mockResponse);
    });
  });

  it('should use correct query key', async () => {
    const mockId = 'apt-123';
    mockAppointmentsService.getDetails.mockResolvedValue({
      id: mockId,
      customerName: 'Cliente 1',
      customerPhone: '11999999999',
      serviceTitle: 'Corte',
      servicePrice: 50,
      serviceDurationMinutes: 30,
      startTime: '2025-10-20T09:00:00Z',
      endTime: '2025-10-20T09:30:00Z',
      status: AppointmentStatus.Pending,
      createdAt: '2025-10-19T10:00:00Z',
    });

    renderHook(() => useAppointmentDetails(mockId), { wrapper });

    await waitFor(() => {
      const cachedData = queryClient.getQueryData(['appointment-details', mockId]);
      expect(cachedData).toBeDefined();
    });
  });

  it('should not refetch on window focus', async () => {
    const mockId = 'apt-123';
    mockAppointmentsService.getDetails.mockResolvedValue({
      id: mockId,
      customerName: 'Cliente 1',
      customerPhone: '11999999999',
      serviceTitle: 'Corte',
      servicePrice: 50,
      serviceDurationMinutes: 30,
      startTime: '2025-10-20T09:00:00Z',
      endTime: '2025-10-20T09:30:00Z',
      status: AppointmentStatus.Pending,
      createdAt: '2025-10-19T10:00:00Z',
    });

    const { result } = renderHook(() => useAppointmentDetails(mockId), { wrapper });

    await waitFor(() => {
      expect(result.current.data).toBeDefined();
    });

    const callCount = mockAppointmentsService.getDetails.mock.calls.length;

    // Simulate window focus
    window.dispatchEvent(new Event('focus'));

    await new Promise(resolve => setTimeout(resolve, 100));

    // Should not refetch
    expect(mockAppointmentsService.getDetails).toHaveBeenCalledTimes(callCount);
  });
});
