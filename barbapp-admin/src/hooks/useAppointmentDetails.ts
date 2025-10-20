/**
 * Hook useAppointmentDetails - Busca detalhes de um agendamento específico
 * 
 * Query habilitada apenas quando o ID é fornecido.
 * Útil para modais de detalhes ou páginas específicas de agendamento.
 * 
 * @example
 * const { data, isLoading, error } = useAppointmentDetails(appointmentId);
 * 
 * // Query desabilitada até ter um ID
 * const { data } = useAppointmentDetails(null);
 */

import { useQuery } from '@tanstack/react-query';
import { appointmentsService } from '@/services';
import type { AppointmentDetails } from '@/types';

export function useAppointmentDetails(id: string | null) {
  return useQuery<AppointmentDetails>({
    queryKey: ['appointment-details', id],
    queryFn: () => appointmentsService.getDetails(id!),
    
    // Apenas busca quando um ID é fornecido
    enabled: !!id,
    
    // Dados de detalhes não ficam stale rapidamente
    // Usuário pode fechar e reabrir modal sem refetch desnecessário
    staleTime: 30000, // 30 segundos
    
    // Não refetch automaticamente ao focar janela
    // Detalhes mudam menos frequentemente que a agenda
    refetchOnWindowFocus: false,
  });
}
