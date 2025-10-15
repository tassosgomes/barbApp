---
status: pending
parallelizable: false
blocked_by: ["1.0","2.0","3.0","6.0","9.0","13.0"]
---

<task_context>
<domain>engine/frontend/pages</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis|temporal</dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 10.0: Página — Barbeiros (lista + formulário + URL state)

## Visão Geral
Construir a página de gestão de Barbeiros: listagem paginada/filtrada, modal/rota de criação/edição, ativar/inativar, erros e toasts.

## Requisitos
- Filtros por nome/status; paginação
- Formulário com validação (zod)
- Estado persistido na URL; reset rápido

## Subtarefas
- [ ] 10.1 Listagem com DataTable + filtros + paginação
- [ ] 10.2 Modal/rota `BarberForm` (create/update)
- [ ] 10.3 Ação de ativar/inativar com confirmação
- [ ] 10.4 Tratamento de erros (409/422) + toasts
- [ ] 10.5 Testes de integração com MSW

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 3.0, 6.0, 9.0, 13.0
- Desbloqueia: —
- Paralelizável: Não (integra vários blocos)

## Detalhes de Implementação
Ver “Página: Barbeiros” e “Contratos de Componentes”.

## Critérios de Sucesso
- Fluxo Criar/Editar/Ativar-Inativar funcional e testado
