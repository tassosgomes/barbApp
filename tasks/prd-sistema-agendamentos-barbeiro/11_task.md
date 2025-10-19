---
status: pending
parallelizable: true
blocked_by: ["8.0"]
---

<task_context>
<domain>engine/frontend/types</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies></dependencies>
<unblocks>"12.0","13.0","14.0","15.0","16.0"</unblocks>
</task_context>

# Tarefa 11.0: Tipos TypeScript e Schemas - Sistema de Agendamentos (Barbeiro)

## Visão Geral
Definir tipos de domínio para Appointment, AppointmentStatus, BarberSchedule e schemas Zod para validação, alinhados com os DTOs do backend.

## Requisitos
- Tipos TypeScript para entidades de agendamento
- Enums para status de agendamento
- Tipos para filtros de data e navegação
- Alinhamento com DTOs do backend (BarberScheduleOutput, AppointmentDetailsOutput)

## Subtarefas
- [ ] 11.1 Criar `src/types/appointment.ts` com tipos:
  - `AppointmentStatus` enum (Pending, Confirmed, Completed, Cancelled)
  - `Appointment` interface
  - `AppointmentDetails` interface
  - `BarberSchedule` interface
- [ ] 11.2 Criar `src/types/schedule-filters.ts` para navegação de data
- [ ] 11.3 Exportar via `src/types/index.ts`
- [ ] 11.4 Validar compatibilidade com contratos do backend

## Sequenciamento
- Bloqueado por: 8.0 (Integração Frontend - Contratos)
- Desbloqueia: 12.0, 13.0, 14.0, 15.0, 16.0
- Paralelizável: Sim

## Detalhes de Implementação

**Tipos principais:**
```typescript
// appointment.ts
export enum AppointmentStatus {
  Pending = 0,
  Confirmed = 1,
  Completed = 2,
  Cancelled = 3
}

export interface Appointment {
  id: string;
  customerName: string;
  serviceTitle: string;
  startTime: string; // ISO 8601
  endTime: string; // ISO 8601
  status: AppointmentStatus;
}

export interface AppointmentDetails extends Appointment {
  customerPhone: string;
  servicePrice: number;
  serviceDurationMinutes: number;
  createdAt: string;
  confirmedAt?: string;
  cancelledAt?: string;
  completedAt?: string;
}

export interface BarberSchedule {
  date: string;
  barberId: string;
  barberName: string;
  appointments: Appointment[];
}
```

## Critérios de Sucesso
- Tipos compilam sem erros
- Compatíveis com respostas do backend
- Exportados corretamente para uso nos componentes
