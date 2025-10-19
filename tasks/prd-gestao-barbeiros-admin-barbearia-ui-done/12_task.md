---
status: completed
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
- [x] 12.1 Listagem da agenda com grouping por barbeiro (opcional)
- [x] 12.2 Filtros + DatePicker + URL sync
- [x] 12.3 Polling e estados de loading/erro
- [x] 12.4 Testes de integração com MSW

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 5.0, 8.0, 9.0, 13.0
- Desbloqueia: —
- Paralelizável: Não

## Detalhes de Implementação
Ver "Página: Agenda" e "Polling Agenda".

## Critérios de Sucesso
- Agenda atualiza automaticamente e respeita filtros

## Status de Conclusão ✅ CONCLUÍDA
- [x] 12.1 Implementação completada
  - [x] SchedulePage.tsx com filtros e navegação de datas
  - [x] ScheduleList.tsx com agrupamento por barbeiro
  - [x] AppointmentCard.tsx com destaque visual e status badges
  - [x] Rota '/agenda' adicionada no router (completando Task 13.0)
- [x] 12.2 Funcionalidades implementadas
  - [x] Filtros por barbeiro, data e status sincronizados com URL
  - [x] Polling automático a cada 30s via useSchedule
  - [x] Navegação entre dias (anterior/próximo/hoje)
  - [x] Destaque visual para horário atual
  - [x] Diferenciação por status (cores/badges)
  - [x] Loading states com skeleton
  - [x] Error states e empty states
- [x] 12.3 Testes implementados e passando (12/12)
- [x] 12.4 Build sem erros
- [x] 12.5 Pronto para revisão
