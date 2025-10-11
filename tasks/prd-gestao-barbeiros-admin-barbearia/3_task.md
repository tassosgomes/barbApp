---
status: pending
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

# Tarefa 3.0: Application - DTOs, Validators e Use Cases (com testes)

## Visão Geral
Implementar DTOs, validações com FluentValidation e use cases: Create/Update/UpdatePhone/Remove/List/GetById de barbeiros, GetTeamSchedule e gestão de serviços. Cobrir com testes unitários via mocks.

## Requisitos
- DTOs Inputs/Outputs conforme Tech Spec
- Validators para criação/edição com normalização de telefone
- Use cases listados, chamando repositórios e UnitOfWork
- Tratamento de exceções de domínio com mensagens claras
- Testes unitários dos use cases (mocks dos repositórios)

## Subtarefas
- [ ] 3.1 Implementar DTOs Inputs/Outputs
- [ ] 3.2 Implementar Validators (FluentValidation)
- [ ] 3.3 Implementar Create/Update/UpdatePhone use cases
- [ ] 3.4 Implementar Remove (validação de agendamentos futuros)
- [ ] 3.5 Implementar List/GetById com paginação e filtros
- [ ] 3.6 Implementar GetTeamSchedule (usar IAppointmentRepository)
- [ ] 3.7 Implementar use cases de serviços (Create/List/Update/Delete)
- [ ] 3.8 Testes unitários dos use cases

## Sequenciamento
- Bloqueado por: 1.0, 2.0
- Desbloqueia: 4.0, 5.0, 6.0, 8.0
- Paralelizável: Não (contratos centrais da API)

## Detalhes de Implementação
- Seguir assinaturas e comportamentos definidos na Tech Spec.
- UnitOfWork.Commit após operações de escrita.
- Log estruturado nos use cases (Information/Warning/Error).

## Critérios de Sucesso
- Testes de application passam cobrindo cenários principais
- Validações rejeitam telefones inválidos e duplicados por barbearia
- Use cases retornam DTOs no formato esperado