---
status: completed
parallelizable: true
blocked_by: ["4.0"]
---

<task_context>
<domain>engine/security</domain>
<type>implementation|testing|documentation</type>
<scope>configuration</scope>
<complexity>low</complexity>
<dependencies>security|http_server</dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 10.0: Segurança e Multi-tenant - Role Barbeiro, TenantContext e Revisão de Acesso

## Visão Geral
Revisar autorização, filtros globais e isolamento por barbearia e barbeiro. Garantir que barbeiro só acessa seus agendamentos na barbearia do contexto.

## Requisitos
- `[Authorize(Roles = "Barbeiro")]` aplicado corretamente
- `ITenantContext` provê `barbeariaId` e `barberId` e são usados em consultas
- Global Query Filter configurado para `Appointment`

## Subtarefas
- [x] 10.1 Revisar controllers e policies
- [x] 10.2 Verificar filtros e testes simples de isolamento
- [x] 10.3 Revisar logs e erros para não vazar dados

## Sequenciamento
- Bloqueado por: 4.0
- Desbloqueia: —
- Paralelizável: Sim

## Detalhes de Implementação
- Seguir regras em `rules/http.md` e `rules/unit-of-work.md`.

## Critérios de Sucesso
- Auditoria rápida confirma ausência de vazamentos e que o isolamento está efetivo

---

- [x] 10.0 [Segurança e Multi-tenant - Role Barbeiro, TenantContext e Revisão de Acesso] ✅ CONCLUÍDA
  - [x] 10.1 Implementação completada
  - [x] 10.2 Definição da tarefa, PRD e tech spec validados
  - [x] 10.3 Análise de regras e conformidade verificadas
  - [x] 10.4 Revisão de código completada
  - [x] 10.5 Pronto para deploy