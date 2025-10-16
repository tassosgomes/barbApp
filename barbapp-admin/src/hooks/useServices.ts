import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { servicesService } from '@/services/services.service';
import type { ServiceFilters } from '@/types';

export function useServices(filters: ServiceFilters) {
  return useQuery({
    queryKey: ['services', filters],
    queryFn: () => servicesService.list(filters),
    placeholderData: keepPreviousData,
  });
}