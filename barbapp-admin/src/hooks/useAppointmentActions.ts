/**
 * Hook useAppointmentActions - Mutations para ações de agendamento
 * 
 * Fornece funções para confirmar, cancelar e concluir agendamentos.
 * Invalida automaticamente o cache da agenda após sucesso.
 * Expõe estados de loading e error para feedback visual.
 * 
 * @example
 * const { confirm, cancel, complete, isLoading } = useAppointmentActions();
 * 
 * // Confirmar agendamento
 * confirm('appointment-id', {
 *   onSuccess: () => toast.success('Agendamento confirmado'),
 *   onError: (error) => toast.error(error.message)
 * });
 */

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { appointmentsService } from '@/services';

interface MutationCallbacks {
  onSuccess?: () => void;
  onError?: (error: Error) => void;
}

export function useAppointmentActions() {
  const queryClient = useQueryClient();
  
  /**
   * Mutation para confirmar um agendamento pendente
   */
  const confirmMutation = useMutation({
    mutationFn: (id: string) => appointmentsService.confirm(id),
    onSuccess: () => {
      // Invalida todas as queries de agenda para forçar refetch
      queryClient.invalidateQueries({ queryKey: ['barber-schedule'] });
      
      // Invalida detalhes do agendamento se estiverem em cache
      queryClient.invalidateQueries({ queryKey: ['appointment-details'] });
    },
  });
  
  /**
   * Mutation para cancelar um agendamento
   */
  const cancelMutation = useMutation({
    mutationFn: (id: string) => appointmentsService.cancel(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['barber-schedule'] });
      queryClient.invalidateQueries({ queryKey: ['appointment-details'] });
    },
  });
  
  /**
   * Mutation para concluir um agendamento
   */
  const completeMutation = useMutation({
    mutationFn: (id: string) => appointmentsService.complete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['barber-schedule'] });
      queryClient.invalidateQueries({ queryKey: ['appointment-details'] });
    },
  });
  
  return {
    /**
     * Confirmar agendamento
     * @param id - ID do agendamento
     * @param callbacks - Callbacks opcionais para onSuccess e onError
     */
    confirm: (id: string, callbacks?: MutationCallbacks) => {
      confirmMutation.mutate(id, {
        onSuccess: callbacks?.onSuccess,
        onError: callbacks?.onError,
      });
    },
    
    /**
     * Cancelar agendamento
     * @param id - ID do agendamento
     * @param callbacks - Callbacks opcionais para onSuccess e onError
     */
    cancel: (id: string, callbacks?: MutationCallbacks) => {
      cancelMutation.mutate(id, {
        onSuccess: callbacks?.onSuccess,
        onError: callbacks?.onError,
      });
    },
    
    /**
     * Concluir agendamento
     * @param id - ID do agendamento
     * @param callbacks - Callbacks opcionais para onSuccess e onError
     */
    complete: (id: string, callbacks?: MutationCallbacks) => {
      completeMutation.mutate(id, {
        onSuccess: callbacks?.onSuccess,
        onError: callbacks?.onError,
      });
    },
    
    /**
     * Estado de loading - true se qualquer mutation estiver em andamento
     */
    isLoading: confirmMutation.isPending || cancelMutation.isPending || completeMutation.isPending,
    
    /**
     * Erro da última mutation que falhou
     */
    error: confirmMutation.error || cancelMutation.error || completeMutation.error,
    
    /**
     * Estados individuais de cada mutation
     */
    confirmState: {
      isPending: confirmMutation.isPending,
      error: confirmMutation.error,
    },
    cancelState: {
      isPending: cancelMutation.isPending,
      error: cancelMutation.error,
    },
    completeState: {
      isPending: completeMutation.isPending,
      error: completeMutation.error,
    },
  };
}
