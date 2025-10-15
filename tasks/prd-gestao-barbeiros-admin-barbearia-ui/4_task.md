---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>engine/frontend/services</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis|http_server</dependencies>
<unblocks>"7.0","11.0","14.0"</unblocks>
</task_context>

# Tarefa 4.0: Serviços de API — Serviços da Barbearia

## Visão Geral
Implementar `src/services/services.service.ts` com CRUD, listagem paginada/filtrada e `toggleActive` (mapeando para DELETE por ora).

## Requisitos
- Métodos: list, getById, create, update, toggleActive
- Normalização de paginação como `barbershop.service.ts`
- Uso de `api.ts` (interceptors JWT)

## Subtarefas
- [ ] 4.1 Criar arquivo de serviço
- [ ] 4.2 Implementar normalização de paginação
- [ ] 4.3 Tratar erros (409/422) para mensagens claras

## Sequenciamento
- Bloqueado por: 2.0
- Desbloqueia: 7.0, 11.0, 14.0
- Paralelizável: Sim

## Detalhes de Implementação
Ver “Endpoints de API” e “Itens de Implementação — Serviços”.

## Critérios de Sucesso
- Lista com filtros funciona contra backend
- Create/update/toggleActive retornam sucesso e erros tratados
