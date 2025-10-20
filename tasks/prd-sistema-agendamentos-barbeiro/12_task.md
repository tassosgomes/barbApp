---
status: completed
parallelizable: true
blocked_by: ["8.0","11.0"]
completed_date: 2025-10-20
---

<task_context>
<domain>engine/frontend/services</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis|http_server</dependencies>
<unblocks>"13.0","14.0","15.0","16.0"</unblocks>
</task_context>

# Tarefa 12.0: Serviços de API - Schedule e Appointments

## Visão Geral
Implementar serviços de API para buscar agenda do barbeiro e executar ações de agendamento (confirmar, cancelar, concluir). Incluir tratamento de erros e concorrência otimista.

## Requisitos
- Service para buscar agenda por data (`GET /api/schedule/my-schedule`)
- Service para buscar detalhes de agendamento (`GET /api/appointments/{id}`)
- Services para ações: confirmar, cancelar, concluir
- Tratamento de erros 403, 404, 409 (conflito)
- Uso de interceptors JWT do `api.ts`

## Subtarefas
- [ ] 12.1 Criar `src/services/schedule.service.ts`:
  - `getMySchedule(date: string): Promise<BarberSchedule>`
- [ ] 12.2 Criar `src/services/appointments.service.ts`:
  - `getAppointmentDetails(id: string): Promise<AppointmentDetails>`
  - `confirmAppointment(id: string): Promise<void>`
  - `cancelAppointment(id: string): Promise<void>`
  - `completeAppointment(id: string): Promise<void>`
- [ ] 12.3 Implementar tratamento de erros específicos:
  - 403 Forbidden: "Você não tem permissão para acessar este agendamento"
  - 404 Not Found: "Agendamento não encontrado"
  - 409 Conflict: "Este agendamento foi modificado. Atualize a página."
- [ ] 12.4 Adicionar tipos de retorno corretos
- [ ] 12.5 Testar com mock data da tarefa 8.0

## Sequenciamento
- Bloqueado por: 8.0 (Contratos), 11.0 (Tipos)
- Desbloqueia: 13.0, 14.0, 15.0, 16.0
- Paralelizável: Sim

## Detalhes de Implementação

**Exemplo de serviço:**
```typescript
// schedule.service.ts
import { api } from '@/lib/api';
import type { BarberSchedule } from '@/types';

export const scheduleService = {
  getMySchedule: async (date: string): Promise<BarberSchedule> => {
    const { data } = await api.get<BarberSchedule>(
      `/schedule/my-schedule`,
      { params: { date } }
    );
    return data;
  }
};

// appointments.service.ts
export const appointmentsService = {
  getDetails: async (id: string): Promise<AppointmentDetails> => {
    const { data } = await api.get<AppointmentDetails>(`/appointments/${id}`);
    return data;
  },
  
  confirm: async (id: string): Promise<void> => {
    await api.post(`/appointments/${id}/confirm`);
  },
  
  cancel: async (id: string): Promise<void> => {
    await api.post(`/appointments/${id}/cancel`);
  },
  
  complete: async (id: string): Promise<void> => {
    await api.post(`/appointments/${id}/complete`);
  }
};
```

**Tratamento de erros:**
- Usar try-catch e verificar `error.response.status`
- Lançar erros com mensagens amigáveis
- Permitir que componentes decidam como exibir

## Critérios de Sucesso
- Services executam requisições corretas para os endpoints
- Erros 403, 404, 409 são tratados com mensagens apropriadas
- Tipos TypeScript corretos em todas as funções
- Funciona com mock data (desenvolvimento)

## ✅ CONCLUSÃO DA TAREFA - VALIDADO

- [x] 12.0 [Serviços de API - Schedule e Appointments] ✅ CONCLUÍDA
  - [x] 12.1 `schedule.service.ts` atualizado - Método `getMySchedule` implementado
  - [x] 12.2 `appointments.service.ts` criado - 4 métodos implementados (getDetails, confirm, cancel, complete)
  - [x] 12.3 Tratamento de erros implementado - Classe `AppointmentError` com mensagens específicas para 403, 404, 409
  - [x] 12.4 Tipos TypeScript corretos - Compatível com tipos de `@/types/appointment`
  - [x] 12.5 Testes implementados - 26 testes unitários (100% passando)
  - [x] Exportações atualizadas em `services/index.ts`
  - [x] Validação completa documentada em `12_task_validation.md`
  - [x] Conformidade com padrões do projeto verificada (code-standard.md, tests-react.md, http.md)
  - [x] Pronto para uso nas tarefas 13.0-16.0 - Services disponíveis para componentes e páginas
