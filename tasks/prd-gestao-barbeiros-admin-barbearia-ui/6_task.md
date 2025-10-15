---
status: pending
parallelizable: true
blocked_by: ["1.0","2.0","3.0"]
---

<task_context>
<domain>engine/frontend/hooks</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"10.0","14.0"</unblocks>
</task_context>

# Tarefa 6.0: Hooks — Barbeiros (queries/mutações)

## Visão Geral
Criar `useBarbers` (lista com filtros/paginação) e `useBarberMutations` (create/update/toggleActive) usando React Query.

## Requisitos
- Query key estável: `['barbers', filters]`
- `keepPreviousData` para paginação
- Invalidação de cache após mutações

## Subtarefas
- [ ] 6.1 Implementar `useBarbers`
- [ ] 6.2 Implementar `useBarberMutations`
- [ ] 6.3 Testes unitários dos hooks com MSW

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 3.0
- Desbloqueia: 10.0, 14.0
- Paralelizável: Sim

## Detalhes de Implementação
Ver “Hooks (novos)” na Tech Spec.

## Critérios de Sucesso
- Hooks retornam dados, estados de loading/erro e refetch
- Mutações invalidam listas corretamente
