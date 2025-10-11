---
status: pending
parallelizable: true
blocked_by: ["3.0"]
---

<task_context>
<domain>engine/application</domain>
<type>implementation|integration|testing</type>
<scope>core_feature|performance</scope>
<complexity>medium</complexity>
<dependencies>temporal|database|http_server</dependencies>
<unblocks>"6.0","8.0","9.0"</unblocks>
</task_context>

# Tarefa 5.0: Agenda Consolidada - Integração com Appointments e Polling (30s)

## Visão Geral
Implementar o use case de agenda consolidada, endpoints necessários e mecanismo de atualização via polling no frontend (contratos e diretrizes). Depende do repositório de agendamentos estar disponível.

## Requisitos
- `GetTeamScheduleUseCase` completo
- Endpoint `GET /api/barbers/schedule` com filtros
- Contrato de resposta conforme Tech Spec
- Diretrizes de polling: 30s, cancelamento ao sair da tela
- Testes unitários do use case com mocks de `IAppointmentRepository`

## Subtarefas
- [ ] 5.1 Implementar `GetTeamScheduleUseCase`
- [ ] 5.2 Expor endpoint na API
- [ ] 5.3 Testes unitários do use case (mock appointments)
- [ ] 5.4 Instruções de polling para frontend

## Sequenciamento
- Bloqueado por: 3.0 (e disponibilidade de IAppointmentRepository do módulo de Agendamentos)
- Desbloqueia: 6.0, 8.0, 9.0
- Paralelizável: Sim (com mocks até integração real)

## Detalhes de Implementação
- Considerar paginação/opcional por barberId e data.
- Otimizar com joins e índices; avaliar cache de curto prazo se necessário.

## Critérios de Sucesso
- Respostas em < 3s para dia atual e equipe típica
- Testes passam e contrato respeitado