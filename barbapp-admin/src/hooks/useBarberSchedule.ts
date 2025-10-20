/**
 * Hook useBarberSchedule - Busca agenda do barbeiro com polling automático
 * 
 * Implementa polling de 10 segundos para manter a agenda atualizada.
 * O polling é desabilitado quando a aba está em background para economizar recursos.
 * 
 * @example
 * const { data, isLoading, error } = useBarberSchedule(new Date());
 */

import { useQuery } from '@tanstack/react-query';
import { scheduleService } from '@/services';
import { format } from 'date-fns';
import type { BarberSchedule } from '@/types';

export function useBarberSchedule(date: Date) {
  const dateStr = format(date, 'yyyy-MM-dd');
  
  return useQuery<BarberSchedule>({
    queryKey: ['barber-schedule', dateStr],
    queryFn: () => scheduleService.getMySchedule(dateStr),
    
    // Polling de 10 segundos conforme TechSpec
    refetchInterval: 10000,
    
    // Não pollar quando a aba está em background
    // Economiza recursos e reduz carga no servidor
    refetchIntervalInBackground: false,
    
    // Considera os dados stale após 9 segundos
    // Garante que na próxima consulta os dados serão atualizados
    staleTime: 9000,
    
    // Mantém dados anteriores enquanto busca novos
    // Evita tela em branco durante refetch
    placeholderData: (previousData) => previousData,
  });
}
