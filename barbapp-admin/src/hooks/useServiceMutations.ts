import { useMutation, useQueryClient } from '@tanstack/react-query';
import { servicesService } from '@/services/services.service';
import type { CreateServiceRequest, UpdateServiceRequest } from '@/types';

export function useServiceMutations() {
  const queryClient = useQueryClient();

  const createService = useMutation({
    mutationFn: (request: CreateServiceRequest) => servicesService.create(request),
    onSuccess: () => {
      // Invalidate services queries to refetch the list
      queryClient.invalidateQueries({ queryKey: ['services'] });
    },
  });

  const updateService = useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateServiceRequest }) =>
      servicesService.update(id, request),
    onSuccess: () => {
      // Invalidate services queries to refetch the list
      queryClient.invalidateQueries({ queryKey: ['services'] });
    },
  });

  const toggleServiceActive = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      servicesService.toggleActive(id, isActive),
    onSuccess: () => {
      // Invalidate services queries to refetch the list
      queryClient.invalidateQueries({ queryKey: ['services'] });
    },
  });

  return {
    createService,
    updateService,
    toggleServiceActive,
  };
}