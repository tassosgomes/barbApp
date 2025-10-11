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
<dependencies>database</dependencies>
<unblocks>"2.0","3.0","4.0","6.0"</unblocks>
</task_context>

# Tarefa 1.0: Domain - Entidades, VOs, Exceções e Repositórios (com testes)

## Visão Geral
Implementar os artefatos de domínio para Gestão de Barbeiros: Value Object `PhoneNumber`, entidades `Barber` e `BarbershopService`, exceções de domínio e contratos de repositório. Cobrir com testes unitários conforme padrão do projeto.

## Requisitos
- Value Object `PhoneNumber` com Normalize/IsValid/Format
- Entidade `Barber` com regras de criação, update, updatePhone, activate/deactivate
- Entidade `BarbershopService` com validações e activate/deactivate
- Exceções: `BarberNotFoundException`, `DuplicateBarberException`, `BarberHasFutureAppointmentsException`
- Interfaces: `IBarberRepository`, `IBarbershopServiceRepository`, `IAppointmentRepository` (contrato)
- Testes unitários de domínio abrangendo cenários positivos e negativos

## Subtarefas
- [ ] 1.1 Implementar Value Object `PhoneNumber`
- [ ] 1.2 Implementar entidade `Barber`
- [ ] 1.3 Implementar entidade `BarbershopService`
- [ ] 1.4 Implementar exceções customizadas
- [ ] 1.5 Definir interfaces de repositório (contratos)
- [ ] 1.6 Criar testes unitários para VO e Entidades

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 2.0, 3.0, 4.0, 6.0
- Paralelizável: Não (primeiro tijolo da arquitetura)

## Detalhes de Implementação
- Seguir definições e assinaturas da Tech Spec em `tasks/prd-gestao-barbeiros-admin-barbearia/techspec.md`.
- Padrões de nomenclatura e limites de classe/método em `rules/code-standard.md`.
- Testes: xUnit + FluentAssertions; AAA pattern.

## Critérios de Sucesso
- Testes de domínio passam (Create/Update/Deactivate e validações de PhoneNumber)
- Código segue Clean Architecture e padrões do repositório
- Assinaturas compatíveis com a camada Application prevista