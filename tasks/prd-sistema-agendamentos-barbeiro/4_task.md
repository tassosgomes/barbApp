---
status: pending
parallelizable: true
blocked_by: ["3.0"]
---

<task_context>
<domain>engine/api</domain>
<type>implementation|documentation|testing</type>
<scope>core_feature|configuration</scope>
<complexity>medium</complexity>
<dependencies>http_server|security</dependencies>
<unblocks>"6.0","8.0","10.0"</unblocks>
</task_context>

# Tarefa 4.0: API - ScheduleController e AppointmentsController + Swagger

## Visão Geral
Implementar controladores REST para agenda do barbeiro e ações de agendamentos, com autorização por role `Barbeiro` e documentação Swagger.

## Requisitos
- Controller `ScheduleController` (GET my-schedule)
- Controller `AppointmentsController` (GET by id, POST confirm/cancel/complete)
- `[Authorize(Roles = "Barbeiro")]` em todos os endpoints
- Tratamento de exceções e códigos 200/400/401/403/404/409
- Swagger com exemplos

## Subtarefas
- [ ] 4.1 Implementar `ScheduleController`
- [ ] 4.2 Implementar `AppointmentsController`
- [ ] 4.3 Configurar Authorization e policies
- [ ] 4.4 Documentar no Swagger

## Sequenciamento
- Bloqueado por: 3.0
- Desbloqueia: 6.0, 8.0, 10.0
- Paralelizável: Sim

## Detalhes de Implementação
- Extrair `(barbeariaId, barberId)` do `ITenantContext` via middleware.

## Critérios de Sucesso
- Endpoints disponíveis e documentados; respostas corretas e autorizadas