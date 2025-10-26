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
<unblocks>"2.0","3.0","4.0","6.0","7.0"</unblocks>
</task_context>

# Tarefa 1.0: Domain - Entidades Cliente/Agendamento, Enum e Regras (com testes)

## Visão Geral
Implementar as entidades e regras de domínio do módulo de Cadastro e Agendamentos do Cliente conforme Tech Spec: `Cliente`, `Agendamento` e `StatusAgendamento`. Garantir validações de domínio (nome/telefone, data futura, duração, transições de status) e cobrir com testes unitários, seguindo os padrões do repositório.

## Requisitos
- Entidade `Cliente` com validações de nome e telefone (normalização para apenas dígitos; 10-11 dígitos)
- Método `ValidarNomeLogin` (case-insensitive) na entidade `Cliente`
- Entidade `Agendamento` com validação de data futura, duração válida (1..480), e status inicial `Pendente`
- Transições: `Confirmar` (Pendente→Confirmado), `Cancelar` (Pendente/Confirmado→Cancelado, não passado), `Concluir` (Confirmado→Concluído)
- Enum `StatusAgendamento` com valores Pendente=1, Confirmado=2, Concluido=3, Cancelado=4
- Exceções de domínio com mensagens explícitas (ex.: "Telefone inválido", "Data/hora deve ser futura")
- Navegações previstas para EF (Cliente→Agendamentos; Agendamento→Cliente/Barbeiro/Servico)

## Subtarefas
- [x] 1.1 Implementar entidade `Cliente` com validações e método `ValidarNomeLogin`
- [x] 1.2 Implementar entidade `Agendamento` com validações e transições
- [x] 1.3 Definir enum `StatusAgendamento`
- [x] 1.4 Criar exceções de domínio (ex.: `DomainException`) - Reutilizado ValidationException existente
- [x] 1.5 Criar testes unitários para `Cliente` (casos válidos e inválidos)
- [x] 1.6 Criar testes unitários para `Agendamento` (criação, confirmar, cancelar, concluir e inválidos)

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 2.0, 3.0, 4.0, 6.0, 7.0
- Paralelizável: Não (base de regras de negócio)

## Detalhes de Implementação
- Referência: `tasks/prd-cadastro-agendamento-cliente/techspec.md` (seções: Modelos de Dados, Abordagem de Testes)
- Padrões de código: `rules/code-standard.md`
- Testes: xUnit + FluentAssertions, padrão AAA, nomes `Metodo_Cenario_ResultadoEsperado`
- Limites: métodos < 50 linhas; classes < 300 linhas; early return; baixo acoplamento

## Critérios de Sucesso
- Testes unitários de domínio passam cobrindo cenários positivos e negativos listados na Tech Spec
- Assinaturas e regras aderentes à Tech Spec para posterior uso nos Use Cases
- Código legível e alinhado aos padrões do repositório
