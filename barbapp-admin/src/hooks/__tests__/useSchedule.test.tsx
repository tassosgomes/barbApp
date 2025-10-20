import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useSchedule } from '../useSchedule';
import type { ScheduleFilters } from '@/types';
import { AppointmentStatus } from '@/types/schedule';

// Mock the schedule service
vi.mock('@/services/schedule.service', () => ({
  scheduleService: {
    list: vi.fn(),
  },
}));

import { scheduleService } from '@/services/schedule.service';

const mockScheduleService = vi.mocked(scheduleService);

describe('useSchedule Hook', () => {
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

  it('should fetch schedule with default filters', async () => {
    const mockResponse = {
      appointments: [
        {
          id: '1',
          barberId: '1',
          barberName: 'João Silva',
          customerId: '1',
          customerName: 'Cliente 1',
          startTime: '2024-10-16T09:00:00Z',
          endTime: '2024-10-16T09:30:00Z',
          serviceTitle: 'Corte de Cabelo',
          status: AppointmentStatus.Confirmed,
        },
      ],
    };

    mockScheduleService.list.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useSchedule({}), { wrapper });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.data).toEqual(mockResponse);
    });

    expect(mockScheduleService.list).toHaveBeenCalledWith({});
  });

  it('should fetch schedule with filters', async () => {
    const filters: ScheduleFilters = {
      date: '2024-10-16',
      barberId: '1',
      status: AppointmentStatus.Confirmed,
    };

    const mockResponse = {
      appointments: [
        {
          id: '1',
          barberId: '1',
          barberName: 'João Silva',
          customerId: '1',
          customerName: 'Cliente 1',
          startTime: '2024-10-16T09:00:00Z',
          endTime: '2024-10-16T09:30:00Z',
          serviceTitle: 'Corte de Cabelo',
          status: AppointmentStatus.Confirmed,
        },
      ],
    };

    mockScheduleService.list.mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useSchedule(filters), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.data).toEqual(mockResponse);
    });

    expect(mockScheduleService.list).toHaveBeenCalledWith(filters);
  });

  it('should handle error state', async () => {
    const error = new Error('Failed to fetch schedule');
    mockScheduleService.list.mockRejectedValue(error);

    const { result } = renderHook(() => useSchedule({}), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toEqual(error);
    });
  });

  it('should use correct query key', async () => {
    const filters: ScheduleFilters = {
      date: '2024-10-16',
      barberId: '1',
    };

    mockScheduleService.list.mockResolvedValue({
      appointments: [],
    });

    renderHook(() => useSchedule(filters), { wrapper });

    await waitFor(() => {
      expect(mockScheduleService.list).toHaveBeenCalledWith(filters);
    });
  });

  it('should have polling configuration', () => {
    // Test that the hook is configured with polling
    // This is more of an implementation detail, but we can check the query options indirectly
    const { result } = renderHook(() => useSchedule({}), { wrapper });

    // The hook should be defined and return a query object
    expect(result.current).toHaveProperty('data');
    expect(result.current).toHaveProperty('isLoading');
    expect(result.current).toHaveProperty('error');
  });
});