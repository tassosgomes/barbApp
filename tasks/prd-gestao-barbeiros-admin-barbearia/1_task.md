---
status: completed
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

# Tarefa 1.0: Domain - Entidades, VOs, Exceções e Repositórios (com testes) ✅ CONCLUÍDA

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
- [x] 1.1 Implementar entidade `Barber` com `Email` e `PasswordHash`. ✅
- [x] 1.2 Implementar entidade `BarbershopService`. ✅
- [x] 1.3 Implementar exceções customizadas. ✅
- [x] 1.4 Definir interfaces de repositório (contratos atualizados). ✅
- [x] 1.5 Criar testes unitários para Entidades e VOs. ✅

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 2.0, 3.0, 4.0, 6.0
- Paralelizável: Não (primeiro tijolo da arquitetura)

## Detalhes de Implementação
- Seguir definições e assinaturas da Tech Spec atualizada.
- Padrões de nomenclatura e limites de classe/método em `rules/code-standard.md`.
- Testes: xUnit + FluentAssertions; AAA pattern.

## Critérios de Sucesso
- [x] Testes de domínio passam (Create/Update/Deactivate e validações de Email/Senha). ✅
- [x] Código segue Clean Architecture e padrões do repositório. ✅
- [x] Assinaturas compatíveis com a camada Application prevista. ✅
- [x] Magic numbers refatorados para constantes. ✅
- [x] 132 testes unitários passando (100% cobertura). ✅

## Status Final
✅ **CONCLUÍDA** - Tarefa revisada e aprovada em 15/10/2025  
📄 Relatório de revisão: `1_task_review.md`