import { useQuery } from '@tanstack/react-query';
import { scheduleService } from '@/services';
import type { ScheduleFilters } from '@/types';

export function useSchedule(filters: ScheduleFilters) {
  return useQuery({
    queryKey: ['schedule', filters],
    queryFn: () => scheduleService.list(filters),
    refetchInterval: 30_000,
    staleTime: 25_000,
  });
}