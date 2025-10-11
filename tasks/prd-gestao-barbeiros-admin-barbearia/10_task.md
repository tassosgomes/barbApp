---
status: pending
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

# Tarefa 10.0: Segurança e LGPD - Autorização, Filtros Globais e Mascaramento

## Visão Geral
Revisar todos os endpoints e logs para conformidade com segurança e LGPD: roles corretos, filtros globais aplicados, mascaramento de telefones.

## Requisitos
- `[Authorize(Roles = "AdminBarbearia")]` aplicado
- Global Query Filters verificados em todos os acessos a Barber/Services
- Telefones mascarados em logs e não expostos em exceções

## Subtarefas
- [ ] 10.1 Revisar controllers e policies
- [ ] 10.2 Verificar filtros globais com testes simples
- [ ] 10.3 Revisar logs e mensagens de erro

## Sequenciamento
- Bloqueado por: 4.0
- Desbloqueia: —
- Paralelizável: Sim

## Detalhes de Implementação
- Seguir `rules/http.md`, `rules/logging.md` e Tech Spec.

## Critérios de Sucesso
- Auditoria rápida não encontra violações de acesso ou vazamento de dados