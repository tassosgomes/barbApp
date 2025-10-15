---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>engine/frontend/services</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>external_apis|http_server|temporal</dependencies>
<unblocks>"8.0","12.0","14.0"</unblocks>
</task_context>

# Tarefa 5.0: Serviço de API — Agenda da Equipe

## Visão Geral
Implementar `src/services/schedule.service.ts` para obter a agenda consolidada com filtros por barbeiro/data/status.

## Requisitos
- Método: list(filters)
- Suporte a filtros `date`, `barberId`, `status`
- Datas ISO, exibição local será feita na UI

## Subtarefas
- [ ] 5.1 Criar arquivo de serviço
- [ ] 5.2 Implementar função `list`
- [ ] 5.3 Tratar erros de rede e tempo limite

## Sequenciamento
- Bloqueado por: 2.0
- Desbloqueia: 8.0, 12.0, 14.0
- Paralelizável: Sim

## Detalhes de Implementação
Ver “Endpoints de API — Agenda” e “Polling Agenda” na Tech Spec.

## Critérios de Sucesso
- Recupera agenda com filtros
- Resiste a falhas transitórias (retry)
