---
status: completed
parallelizable: false
blocked_by: ["1.0","2.0"]
---

<task_context>
<domain>engine/application</domain>
<type>implementation|testing</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database|http_server|temporal</dependencies>
<unblocks>"4.0","5.0","6.0","8.0"</unblocks>
</task_context>

# Tarefa 3.0: Application - DTOs, Validators e Use Cases (com testes) ✅ CONCLUÍDA

## Visão Geral
Implementar DTOs, validações com FluentValidation e use cases: Create/Update/Remove/List/GetById de barbeiros (com auth por email/senha), GetTeamSchedule e gestão de serviços. Cobrir com testes unitários via mocks.

## Requisitos
- DTOs Inputs/Outputs conforme Tech Spec atualizada (usando Email/Senha).
- Validators para criação/edição com validação de email e senha.
- Use cases listados, chamando repositórios e UnitOfWork.
- `RemoveBarberUseCase` deve implementar a lógica de cancelamento de agendamentos futuros.
- Tratamento de exceções de domínio com mensagens claras.
- Testes unitários dos use cases (mocks dos repositórios).

## Subtarefas
- [x] 3.1 Implementar DTOs Inputs/Outputs (com Email/Senha). ✅
- [x] 3.2 Implementar Validators (FluentValidation para Email/Senha). ✅
- [x] 3.3 Implementar Create/Update use cases. ✅
- [x] 3.4 Implementar `RemoveBarberUseCase` (com lógica de cancelamento de agendamentos). ✅
- [x] 3.5 Implementar List/GetById com paginação e filtros. ✅
- [x] 3.6 Implementar GetTeamSchedule (usar IAppointmentRepository). ✅
- [x] 3.7 Implementar use cases de serviços (Create/List/Update/Delete). ✅
- [x] 3.8 Testes unitários dos use cases, incluindo o cenário de remoção. ✅

## Sequenciamento
- Bloqueado por: 1.0, 2.0
- Desbloqueia: 4.0, 5.0, 6.0, 8.0
- Paralelizável: Não (contratos centrais da API)

## Detalhes de Implementação
- Seguir assinaturas e comportamentos definidos na Tech Spec atualizada.
- `RemoveBarberUseCase` precisará orquestrar a busca de agendamentos futuros, a atualização de seu status e a desativação do barbeiro dentro de uma transação.
- UnitOfWork.Commit após operações de escrita.
- Log estruturado nos use cases (Information/Warning/Error).

## Critérios de Sucesso
- Testes de application Passam cobrindo cenários principais.
- Validações rejeitam emails inválidos e duplicados por barbearia.
- `RemoveBarberUseCase` cancela agendamentos antes de desativar o barbeiro.
- Use cases retornam DTOs no formato esperado.

## Status Final
✅ **CONCLUÍDA** - Tarefa revisada e aprovada em 15/10/2025  
📄 Relatório de revisão: `3_task_review.md`