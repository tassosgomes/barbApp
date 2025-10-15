---
status: pending
parallelizable: true
blocked_by: ["1.0","2.0","4.0"]
---

<task_context>
<domain>engine/frontend/hooks</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"11.0","14.0"</unblocks>
</task_context>

# Tarefa 7.0: Hooks — Serviços (queries/mutações)

## Visão Geral
Criar `useServices` (lista com filtros/paginação) e `useServiceMutations` (create/update/toggleActive) usando React Query.

## Requisitos
- Query key estável: `['services', filters]`
- `keepPreviousData` para paginação
- Invalidação de cache após mutações

## Subtarefas
- [ ] 7.1 Implementar `useServices`
- [ ] 7.2 Implementar `useServiceMutations`
- [ ] 7.3 Testes unitários dos hooks com MSW

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 4.0
- Desbloqueia: 11.0, 14.0
- Paralelizável: Sim

## Detalhes de Implementação
Ver “Hooks (novos)” na Tech Spec.

## Critérios de Sucesso
- Hooks retornam dados, estados de loading/erro e refetch
- Mutações invalidam listas corretamente
