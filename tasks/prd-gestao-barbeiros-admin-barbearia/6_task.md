---
status: pending
parallelizable: true
blocked_by: ["4.0","5.0"]
---

<task_context>
<domain>engine/testing/integration</domain>
<type>testing</type>
<scope>core_feature|configuration</scope>
<complexity>medium</complexity>
<dependencies>database|http_server</dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 6.0: Testes de Integração - CRUD, Isolamento Multi-tenant e Agenda

## Visão Geral
Configurar TestContainers e implementar testes de integração para validar a API ponta a ponta, incluindo isolamento multi-tenant e regras de negócio críticas.

## Requisitos
- TestContainers com Postgres 16.
- WebApplicationFactory configurado com migrations.
- Cenários: criação, duplicidade de **email**, listagem com filtros, remoção que **cancela agendamentos futuros**, agenda consolidada, isolamento multi-tenant.

## Subtarefas
- [ ] 6.1 Configurar TestContainers e factory.
- [ ] 6.2 Testes CRUD de Barbers (com auth por email).
- [ ] 6.3 Teste de isolamento multi-tenant.
- [ ] 6.4 Teste de remoção que cancela agendamentos futuros.
- [ ] 6.5 Teste da agenda consolidada.

## Sequenciamento
- Bloqueado por: 4.0, 5.0
- Desbloqueia: —
- Paralelizável: Sim

## Detalhes de Implementação
- Reutilizar exemplos da Tech Spec atualizada; mascarar dados sensíveis nos logs.

## Critérios de Sucesso
- Todos os testes de integração passam e cobrem os cenários chave atualizados.