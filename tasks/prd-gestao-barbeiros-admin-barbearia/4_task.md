---
status: completed
parallelizable: true
blocked_by: ["3.0"]
---

<task_context>
<domain>engine/api</domain>
<type>implementation|documentation|testing</type>
<scope>core_feature|configuration</scope>
<complexity>medium</complexity>
<dependencies>http_server|security</dependencies>
<unblocks>"6.0","8.0","10.0"</unblocks>
</task_context>

# Tarefa 4.0: API - Endpoints Barbers e Barbershop Services + Autorização + Swagger

## Visão Geral
Implementar controllers REST para gestão de barbeiros e serviços, configurar autorização por role `AdminBarbearia` e documentar via Swagger/OpenAPI.

## Requisitos
- Controllers: `BarbersController`, `BarbershopServicesController`
- Autorização `[Authorize(Roles = "AdminBarbearia")]`
- Endpoints e contratos conforme Tech Spec
- Middleware de exceções global com códigos 400/401/403/404/409/422
- Documentação Swagger com exemplos de request/response

## Subtarefas
- [x] 4.1 Implementar BarbersController (CRUD + schedule)
- [x] 4.2 Implementar BarbershopServicesController
- [x] 4.3 Configurar Authorization e policies
- [x] 4.4 Middleware de exception handling
- [x] 4.5 Documentar endpoints no Swagger

## Sequenciamento
- Bloqueado por: 3.0
- Desbloqueia: 6.0, 8.0, 10.0
- Paralelizável: Sim (após 3.0, paralelo com 5.0 parcial e 7.0/8.0)

## Detalhes de Implementação
- Mapear DTOs de entrada/saída dos use cases para controllers.
- Garantir extração do `barbeariaId` via TenantMiddleware.
- Validar paginação `limit/offset` com limites razoáveis.

## Critérios de Sucesso
- Swagger apresenta todos os endpoints e exemplos
- Requisições autenticadas funcionam conforme contratos
- Códigos HTTP corretos por cenário

## Status Final
✅ **CONCLUÍDA** - Tarefa revisada e aprovada em 15/10/2025  
📄 Relatório de revisão: `4_task_review.md`