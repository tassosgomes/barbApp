---
status: completed
parallelizable: true
blocked_by: []
---

<task_context>
<domain>engine/frontend/types</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies></dependencies>
<unblocks>"3.0","4.0","5.0","6.0","7.0","8.0","10.0","11.0","12.0"</unblocks>
</task_context>

# Tarefa 2.0: Tipos TypeScript e Schemas Zod

## Visão Geral
Definir tipos de domínio (Barber, BarbershopService, Appointment) e filtros, além de schemas Zod para validação de formulários de Barbeiro e Serviço.

## Requisitos
- `src/types/{barber.ts, service.ts, schedule.ts, filters.ts}`
- `src/schemas/{barber.ts, service.ts}` com validações

## Subtarefas
- [x] 2.1 Criar tipos TS conforme Tech Spec
- [x] 2.2 Implementar schemas Zod (create/update)
- [x] 2.3 Exportar via `src/types/index.ts`

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 10.0, 11.0, 12.0
- Paralelizável: Sim

## Detalhes de Implementação
Ver “Modelos de Dados” e “Schemas Zod (excertos)” na Tech Spec.

## Critérios de Sucesso
- Tipos e schemas compilam
- Formulários conseguem usar schemas para validação
