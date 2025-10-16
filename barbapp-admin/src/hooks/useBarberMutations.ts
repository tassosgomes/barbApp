import { useMutation, useQueryClient } from '@tanstack/react-query';
import { barbersService } from '@/services/barbers.service';
import type { CreateBarberRequest, UpdateBarberRequest } from '@/types';

export function useBarberMutations() {
  const queryClient = useQueryClient();

  const createBarber = useMutation({
    mutationFn: (request: CreateBarberRequest) => barbersService.create(request),
    onSuccess: () => {
      // Invalidate barbers queries to refetch the list
      queryClient.invalidateQueries({ queryKey: ['barbers'] });
    },
  });

  const updateBarber = useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateBarberRequest }) =>
      barbersService.update(id, request),
    onSuccess: () => {
      // Invalidate barbers queries to refetch the list
      queryClient.invalidateQueries({ queryKey: ['barbers'] });
    },
  });

  const toggleBarberActive = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      barbersService.toggleActive(id, isActive),
    onSuccess: () => {
      // Invalidate barbers queries to refetch the list
      queryClient.invalidateQueries({ queryKey: ['barbers'] });
    },
  });

  return {
    createBarber,
    updateBarber,
    toggleBarberActive,
  };
}