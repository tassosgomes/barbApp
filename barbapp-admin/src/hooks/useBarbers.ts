import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { barbersService } from '@/services/barbers.service';
import type { BarberFilters } from '@/types';

export function useBarbers(filters: BarberFilters) {
  return useQuery({
    queryKey: ['barbers', filters],
    queryFn: () => barbersService.list(filters),
    placeholderData: keepPreviousData,
  });
}