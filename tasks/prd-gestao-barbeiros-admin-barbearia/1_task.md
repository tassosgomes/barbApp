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
Implementar os artefatos de domínio para Gestão de Barbeiros: entidade `Barber` (com autenticação por email/senha), entidade `BarbershopService`, exceções de domínio e contratos de repositório. Cobrir com testes unitários.

## Requisitos
- Entidade `Barber` com `Email`, `PasswordHash` e regras de criação/atualização (soft delete).
- O Value Object `PhoneNumber` será usado apenas para contato.
- Entidade `BarbershopService` com validações e activate/deactivate.
- Exceções: `BarberNotFoundException`, `DuplicateBarberException` (baseada no email).
- Interfaces: `IBarberRepository` (com `GetByEmailAsync`), `IBarbershopServiceRepository`, `IAppointmentRepository` (contrato).
- Testes unitários de domínio abrangendo cenários positivos e negativos para as novas regras.

## Subtarefas
- [ ] 1.1 Implementar entidade `Barber` com `Email` e `PasswordHash`.
- [ ] 1.2 Implementar entidade `BarbershopService`.
- [ ] 1.3 Implementar exceções customizadas.
- [ ] 1.4 Definir interfaces de repositório (contratos atualizados).
- [ ] 1.5 Criar testes unitários para Entidades e VOs.

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 2.0, 3.0, 4.0, 6.0
- Paralelizável: Não (primeiro tijolo da arquitetura)

## Detalhes de Implementação
- Seguir definições e assinaturas da Tech Spec atualizada.
- Padrões de nomenclatura e limites de classe/método em `rules/code-standard.md`.
- Testes: xUnit + FluentAssertions; AAA pattern.

## Critérios de Sucesso
- Testes de domínio passam (Create/Update/Deactivate e validações de Email/Senha).
- Código segue Clean Architecture e padrões do repositório.
- Assinaturas compatíveis com a camada Application prevista.