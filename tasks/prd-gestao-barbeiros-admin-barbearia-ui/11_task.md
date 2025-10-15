---
status: pending
parallelizable: false
blocked_by: ["1.0","2.0","4.0","7.0","9.0","13.0"]
---

<task_context>
<domain>engine/frontend/pages</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis|temporal</dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 11.0: Página — Serviços (lista + formulário + URL state)

## Visão Geral
Construir a página de gestão de Serviços: listagem paginada/filtrada, modal/rota de criação/edição, ativar/inativar, erros e toasts.

## Requisitos
- Filtros por nome/status; paginação
- Formulário com validação (zod)
- Estado persistido na URL; reset rápido

## Subtarefas
- [ ] 11.1 Listagem com DataTable + filtros + paginação
- [ ] 11.2 Modal/rota `ServiceForm` (create/update)
- [ ] 11.3 Ação de ativar/inativar com confirmação
- [ ] 11.4 Tratamento de erros (409/422) + toasts
- [ ] 11.5 Testes de integração com MSW

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 4.0, 7.0, 9.0, 13.0
- Desbloqueia: —
- Paralelizável: Não

## Detalhes de Implementação
Ver “Página: Serviços” e “Contratos de Componentes”.

## Critérios de Sucesso
- Fluxo Criar/Editar/Ativar-Inativar funcional e testado
