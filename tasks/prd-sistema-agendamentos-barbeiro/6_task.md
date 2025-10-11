---
status: pending
parallelizable: true
blocked_by: ["4.0"]
---

<task_context>
<domain>engine/testing/integration</domain>
<type>testing</type>
<scope>core_feature|configuration</scope>
<complexity>medium</complexity>
<dependencies>database|http_server</dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 6.0: Testes de Integração - Autorização, Conflitos (409) e Isolamento

## Visão Geral
Criar testes de integração com TestContainers para validar autorização do role Barbeiro, conflitos de status e isolamento multi-tenant por barbearia e barbeiro.

## Requisitos
- Setup de Postgres com TestContainers
- Testes dos endpoints (my-schedule, get details, confirm/cancel/complete)
- Verificações de 403/404/409 apropriados e 200 em sucesso

## Subtarefas
- [ ] 6.1 Configurar TestContainers e WebApplicationFactory
- [ ] 6.2 Testes de autorização e acesso restrito
- [ ] 6.3 Testes de conflito de status (optimistic concurrency)
- [ ] 6.4 Testes de isolamento (não acessar agendas de outras barbearias/barbeiros)

## Sequenciamento
- Bloqueado por: 4.0
- Desbloqueia: —
- Paralelizável: Sim

## Detalhes de Implementação
- Criar helpers de autenticação para role Barbeiro.

## Critérios de Sucesso
- Todos os testes de integração passam cobrindo cenários chave