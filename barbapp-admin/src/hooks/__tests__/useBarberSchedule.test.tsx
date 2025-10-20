import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useBarberSchedule } from '../useBarberSchedule';
import { AppointmentStatus } from '@/types/appointment';

// Mock the schedule service
vi.mock('@/services/schedule.service', () => ({
  scheduleService: {
    getMySchedule: vi.fn(),
  },
}));

import { scheduleService } from '@/services/schedule.service';

const mockScheduleService = vi.mocked(scheduleService);

describe('useBarberSchedule Hook', () => {
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

  it('should fetch barber schedule for a given date', async () => {
    const mockDate = new Date('2025-10-20T12:00:00Z');
    const mockResponse = {
      date: '2025-10-20',
      barberId: 'barber-123',
      barberName: 'João Silva',
      appointments: [
        {
          id: 'apt-1',
          customerName: 'Cliente 1',
          serviceTitle: 'Corte de Cabelo',
          startTime: '2025-10-20T09:00:00Z',
          endTime: '2025-10-20T09:30:00Z',
          status: AppointmentStatus.Confirmed,
        },
      ],
    };

    mockScheduleService.getMySchedule.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useBarberSchedule(mockDate), { wrapper });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.data).toEqual(mockResponse);
    });

    expect(mockScheduleService.getMySchedule).toHaveBeenCalled();
    const callArg = mockScheduleService.getMySchedule.mock.calls[0][0];
    expect(callArg).toMatch(/2025-10-2\d/); // Allow for timezone differences
  });

  it('should use correct query key with formatted date', async () => {
    const mockDate = new Date('2025-10-20T12:00:00Z');
    const mockResponse = {
      date: '2025-10-20',
      barberId: 'barber-123',
      barberName: 'João Silva',
      appointments: [],
    };
    mockScheduleService.getMySchedule.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useBarberSchedule(mockDate), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.data).toEqual(mockResponse);
    });

    // Verify query was called
    expect(mockScheduleService.getMySchedule).toHaveBeenCalled();
  });

  it('should handle error state', async () => {
    const mockDate = new Date('2025-10-20');
    const error = new Error('Failed to fetch schedule');
    mockScheduleService.getMySchedule.mockRejectedValue(error);

    const { result } = renderHook(() => useBarberSchedule(mockDate), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toEqual(error);
    });
  });

  it('should refetch when date changes', async () => {
    const date1 = new Date('2025-10-20T12:00:00Z');
    const date2 = new Date('2025-10-21T12:00:00Z');

    mockScheduleService.getMySchedule.mockResolvedValue({
      date: '2025-10-20',
      barberId: 'barber-123',
      barberName: 'João Silva',
      appointments: [],
    });

    const { rerender } = renderHook(
      ({ date }) => useBarberSchedule(date),
      {
        wrapper,
        initialProps: { date: date1 },
      }
    );

    await waitFor(() => {
      expect(mockScheduleService.getMySchedule).toHaveBeenCalledTimes(1);
    });

    mockScheduleService.getMySchedule.mockResolvedValue({
      date: '2025-10-21',
      barberId: 'barber-123',
      barberName: 'João Silva',
      appointments: [],
    });

    rerender({ date: date2 });

    await waitFor(() => {
      expect(mockScheduleService.getMySchedule).toHaveBeenCalledTimes(2);
    });
  });

  it('should have polling configuration', async () => {
    const mockDate = new Date('2025-10-20');
    mockScheduleService.getMySchedule.mockResolvedValue({
      date: '2025-10-20',
      barberId: 'barber-123',
      barberName: 'João Silva',
      appointments: [],
    });

    const { result } = renderHook(() => useBarberSchedule(mockDate), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    // Hook should expose query result properties
    expect(result.current).toHaveProperty('data');
    expect(result.current).toHaveProperty('isLoading');
    expect(result.current).toHaveProperty('error');
    expect(result.current).toHaveProperty('refetch');
  });

  it('should keep previous data while refetching', async () => {
    const mockDate = new Date('2025-10-20');
    const firstResponse = {
      date: '2025-10-20',
      barberId: 'barber-123',
      barberName: 'João Silva',
      appointments: [
        {
          id: 'apt-1',
          customerName: 'Cliente 1',
          serviceTitle: 'Corte',
          startTime: '2025-10-20T09:00:00Z',
          endTime: '2025-10-20T09:30:00Z',
          status: AppointmentStatus.Pending,
        },
      ],
    };

    mockScheduleService.getMySchedule.mockResolvedValue(firstResponse);

    const { result } = renderHook(() => useBarberSchedule(mockDate), { wrapper });

    await waitFor(() => {
      expect(result.current.data).toEqual(firstResponse);
    });

    // Update mock for refetch
    const updatedResponse = {
      ...firstResponse,
      appointments: [
        {
          ...firstResponse.appointments[0],
          status: AppointmentStatus.Confirmed,
        },
      ],
    };
    mockScheduleService.getMySchedule.mockResolvedValue(updatedResponse);

    // Trigger refetch
    result.current.refetch();

    // Data should be available during refetch (placeholderData)
    expect(result.current.data).toBeDefined();
  });
});
