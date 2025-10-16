---
status: completed
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
- [x] 7.1 Implementar `useServices`
- [x] 7.2 Implementar `useServiceMutations`
- [x] 7.3 Testes unitários dos hooks com MSW

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 4.0
- Desbloqueia: 11.0, 14.0
- Paralelizável: Sim

## Detalhes de Implementação
Ver “Hooks (novos)” na Tech Spec.

## Critérios de Sucesso
- Hooks retornam dados, estados de loading/erro e refetch
- Mutações invalidam listas corretamente

## Status de Conclusão ✅ CONCLUÍDA
- [x] 7.1 Implementação completada
- [x] 7.2 Definição da tarefa, PRD e tech spec validados
- [x] 7.3 Análise de regras e conformidade verificadas
- [x] 7.4 Revisão de código completada
- [x] 7.5 Pronto para deploy
