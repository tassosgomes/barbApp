---
status: pending
parallelizable: true
blocked_by: ["1.0","2.0","5.0"]
---

<task_context>
<domain>engine/frontend/hooks</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>temporal|external_apis</dependencies>
<unblocks>"12.0","14.0"</unblocks>
</task_context>

# Tarefa 8.0: Hook — Agenda (polling 30s)

## Visão Geral
Criar `useSchedule` para recuperar a agenda com `refetchInterval: 30_000` e filtros por URL.

## Requisitos
- Query key: `['schedule', filters]`
- `refetchInterval` 30s e `staleTime` ~25s
- Tratamento de erros e estado de loading

## Subtarefas
- [ ] 8.1 Implementar `useSchedule`
- [ ] 8.2 Testes unitários com MSW

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 5.0
- Desbloqueia: 12.0, 14.0
- Paralelizável: Sim

## Detalhes de Implementação
Ver “Polling Agenda” e “Endpoints de API — Agenda”.

## Critérios de Sucesso
- Hook realiza polling e atualiza dados com estabilidade
