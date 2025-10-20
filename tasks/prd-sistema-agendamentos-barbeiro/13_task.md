---
status: pending
parallelizable: true
blocked_by: ["11.0","12.0"]
---

<task_context>
<domain>engine/frontend/hooks</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"14.0","15.0","16.0"</unblocks>
</task_context>

# Tarefa 13.0: React Query Hooks - Agenda e Ações

## Visão Geral
Implementar custom hooks usando React Query para buscar agenda com polling de 10s, buscar detalhes de agendamento e executar mutations (confirmar, cancelar, concluir).

## Requisitos
- Hook `useBarberSchedule` com polling automático de 10 segundos
- Hook `useAppointmentDetails` para detalhes de um agendamento
- Mutations para confirmar, cancelar e concluir agendamentos
- Invalidação automática de queries após mutations
- Tratamento de estados de loading, error e success

## Subtarefas
- [x] 13.1 Criar `src/hooks/useBarberSchedule.ts`:
  - Query com `refetchInterval: 10000`
  - Recebe `date` como parâmetro
  - Desabilita polling quando fora da tela (refetchIntervalInBackground: false)
- [x] 13.2 Criar `src/hooks/useAppointmentDetails.ts`:
  - Query para buscar detalhes
  - Enabled somente quando `id` é fornecido
- [x] 13.3 Criar `src/hooks/useAppointmentActions.ts`:
  - Mutations: `confirmAppointment`, `cancelAppointment`, `completeAppointment`
  - Invalidar query de schedule após sucesso
  - onSuccess callbacks para feedback visual
- [x] 13.4 Implementar cancelamento de polling ao desmontar componente
- [x] 13.5 Adicionar tratamento de erros específicos (403, 404, 409)

## Sequenciamento
- Bloqueado por: 11.0 (Tipos), 12.0 (Services)
- Desbloqueia: 14.0, 15.0, 16.0
- Paralelizável: Sim

## Detalhes de Implementação

**Exemplo de hooks:**
```typescript
// useBarberSchedule.ts
import { useQuery } from '@tanstack/react-query';
import { scheduleService } from '@/services/schedule.service';
import { format } from 'date-fns';

export function useBarberSchedule(date: Date) {
  const dateStr = format(date, 'yyyy-MM-dd');
  
  return useQuery({
    queryKey: ['barber-schedule', dateStr],
    queryFn: () => scheduleService.getMySchedule(dateStr),
    refetchInterval: 10000, // Polling de 10s
    refetchIntervalInBackground: false, // Não pollar em background
    staleTime: 9000 // Considera stale após 9s
  });
}

// useAppointmentActions.ts
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { appointmentsService } from '@/services/appointments.service';

export function useAppointmentActions() {
  const queryClient = useQueryClient();
  
  const confirmMutation = useMutation({
    mutationFn: (id: string) => appointmentsService.confirm(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['barber-schedule'] });
    }
  });
  
  const cancelMutation = useMutation({
    mutationFn: (id: string) => appointmentsService.cancel(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['barber-schedule'] });
    }
  });
  
  const completeMutation = useMutation({
    mutationFn: (id: string) => appointmentsService.complete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['barber-schedule'] });
    }
  });
  
  return {
    confirm: confirmMutation.mutate,
    cancel: cancelMutation.mutate,
    complete: completeMutation.mutate,
    isLoading: confirmMutation.isPending || cancelMutation.isPending || completeMutation.isPending
  };
}
```

**Diretrizes de Polling:**
- Polling de 10 segundos conforme TechSpec
- Desabilitar em background para economizar recursos
- Cancelar automaticamente ao desmontar componente
- Usar AbortController se necessário

## Critérios de Sucesso
- Agenda atualiza automaticamente a cada 10 segundos
- Polling para quando usuário sai da tela
- Mutations invalidam cache e re-buscam dados
- Estados de loading/error são expostos corretamente
- Testes confirmam comportamento de polling e mutations
