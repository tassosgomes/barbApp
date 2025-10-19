---
status: completed
parallelizable: false
blocked_by: ["1.0","2.0"]
completed_date: 2025-10-19
---

<task_context>
<domain>engine/application</domain>
<type>implementation|testing</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database|temporal|http_server</dependencies>
<unblocks>"4.0","5.0","6.0","8.0"</unblocks>
</task_context>

# Tarefa 3.0: Application - DTOs, Validators e Use Cases (com testes)

## Visão Geral
Implementar DTOs e use cases: `GetBarberSchedule`, `GetAppointmentDetails`, `ConfirmAppointment`, `CancelAppointment`, `CompleteAppointment`, com validações e testes unitários.

## Requisitos
- DTOs de saída e entrada conforme Tech Spec
- Validators (FluentValidation) quando aplicável
- Use cases implementados com `IUnitOfWork` e logs estruturados
- Testes unitários com mocks do repositório

## Subtarefas
- [x] 3.1 Implementar DTOs (Outputs e Inputs)
- [x] 3.2 Implementar `GetBarberScheduleUseCase`
- [x] 3.3 Implementar `GetAppointmentDetailsUseCase`
- [x] 3.4 Implementar `ConfirmAppointmentUseCase`
- [x] 3.5 Implementar `CancelAppointmentUseCase`
- [x] 3.6 Implementar `CompleteAppointmentUseCase`
- [x] 3.7 Testes unitários para todos os use cases

## Sequenciamento
- Bloqueado por: 1.0, 2.0
- Desbloqueia: 4.0, 5.0, 6.0, 8.0
- Paralelizável: Não

## Detalhes de Implementação
- Concorrência otimista: lançar 409 em transições inválidas
- Horários sempre em UTC; conversão no frontend

## Critérios de Sucesso
- Testes de application passam com cobertura dos cenários principais