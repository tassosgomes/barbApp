---
status: pending
parallelizable: false
blocked_by: ["1.0","2.0","5.0","8.0","9.0","13.0"]
---

<task_context>
<domain>engine/frontend/pages</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>temporal|external_apis</dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 12.0: Página — Agenda (lista, filtros, polling)

## Visão Geral
Construir a página de Agenda com visão padrão em lista, filtros por barbeiro/data/status e polling a cada 30s.

## Requisitos
- Filtros via URL, navegação anterior/próximo dia
- Destaque de horário atual; cores por status

## Subtarefas
- [ ] 12.1 Listagem da agenda com grouping por barbeiro (opcional)
- [ ] 12.2 Filtros + DatePicker + URL sync
- [ ] 12.3 Polling e estados de loading/erro
- [ ] 12.4 Testes de integração com MSW

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 5.0, 8.0, 9.0, 13.0
- Desbloqueia: —
- Paralelizável: Não

## Detalhes de Implementação
Ver “Página: Agenda” e “Polling Agenda”.

## Critérios de Sucesso
- Agenda atualiza automaticamente e respeita filtros
