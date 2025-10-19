---
status: pending
parallelizable: true
blocked_by: ["1.0","2.0","3.0","4.0","5.0","6.0","7.0","8.0","9.0","10.0","11.0","12.0","13.0"]
---

<task_context>
<domain>engine/frontend/testing</domain>
<type>testing</type>
<scope>quality</scope>
<complexity>medium</complexity>
<dependencies>external_apis|temporal</dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 14.0: Testes (unit, integração, e2e)

## Visão Geral
Cobrir componentes, hooks e páginas com Vitest + RTL + user-event, integração com MSW e E2E com Playwright para os fluxos principais.

## Requisitos
- Unit tests para hooks e componentes
- Integração com MSW para páginas
- Playwright para fluxos: criar barbeiro, criar serviço, visualizar agenda

## Subtarefas
- [ ] 14.1 Unit tests (hooks e componentes)
- [ ] 14.2 Integração/MSW (páginas)
- [ ] 14.3 E2E/Playwright (fluxos)
- [ ] 14.4 Coverage report e checklist de qualidade

## Sequenciamento
- Bloqueado por: 1.0–13.0
- Desbloqueia: —
- Paralelizável: Sim (após features estarem prontas)

## Detalhes de Implementação
Ver `rules/tests-react.md` e seção “Abordagem de Testes”.

## Critérios de Sucesso
- Testes verdes e cobertura adequada das rotas/fluxos críticos
