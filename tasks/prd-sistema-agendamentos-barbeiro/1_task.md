---
status: pending
parallelizable: false
blocked_by: []
---

<task_context>
<domain>engine/domain</domain>
<type>implementation|testing</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database|temporal</dependencies>
<unblocks>"2.0","3.0","4.0","6.0"</unblocks>
</task_context>

# Tarefa 1.0: Domain - Entidade Appointment, Enum e Exceções (com testes)

## Visão Geral
Implementar a entidade `Appointment`, enum `AppointmentStatus` e exceções `AppointmentNotFoundException`, `InvalidAppointmentStatusTransitionException`. Cobrir transições de status com testes unitários.

## Requisitos
- Entidade `Appointment` com propriedades e métodos: Confirm, Cancel, Complete
- Enum `AppointmentStatus` (Pending, Confirmed, Completed, Cancelled)
- Exceções de domínio conforme Tech Spec
- Testes unitários de máquina de estados e validações

## Subtarefas
- [ ] 1.1 Implementar enum `AppointmentStatus`
- [ ] 1.2 Implementar entidade `Appointment` com regras
- [ ] 1.3 Implementar exceções customizadas
- [ ] 1.4 Testes unitários cobrindo transições válidas/ inválidas

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 2.0, 3.0, 4.0, 6.0
- Paralelizável: Não

## Detalhes de Implementação
- Armazenar horários em UTC; validação de Complete() não antes do StartTime.
- Manter UpdatedAt em mutações; setar timestamps ConfirmedAt/CancelledAt/CompletedAt.

## Critérios de Sucesso
- Testes de domínio passam (incluindo exceções esperadas)
- API de domínio compatível com use cases planejados