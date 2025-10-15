---
status: pending
parallelizable: true
blocked_by: []
---

<task_context>
<domain>engine/frontend/components</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies></dependencies>
<unblocks>"10.0","11.0","12.0"</unblocks>
</task_context>

# Tarefa 9.0: Componentes Compartilhados

## Visão Geral
Implementar componentes comuns: `DataTable`, `FiltersBar`, `StatusBadge`, `ConfirmDialog`, `DatePicker`, `ToastProvider` (Radix/shadcn + Tailwind).

## Requisitos
- Acessibilidade básica (labels, foco)
- Estilização Tailwind conforme padrão do repo

## Subtarefas
- [ ] 9.1 DataTable com paginação básica
- [ ] 9.2 FiltersBar com sincronização de URL
- [ ] 9.3 StatusBadge para Ativo/Inativo
- [ ] 9.4 ConfirmDialog para inativação
- [ ] 9.5 DatePicker para filtros de data
- [ ] 9.6 ToastProvider e helpers de notificação

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 10.0, 11.0, 12.0
- Paralelizável: Sim

## Detalhes de Implementação
Ver “Especificação de UI e Interações” e “Itens de Implementação — Componentes”.

## Critérios de Sucesso
- Componentes reutilizáveis prontos e testados visualmente
