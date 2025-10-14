---
status: completed
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>engine/infra/http_server</domain>
<type>implementation|integration</type>
<scope>middleware</scope>
<complexity>medium</complexity>
<dependencies>http_server|external_apis</dependencies>
<unblocks>"9.0","10.0"</unblocks>
</task_context>

# Tarefa 2.0: Backend - Captura global e enriquecimento de escopo

## Visão Geral
Capturar exceções no `GlobalExceptionHandlerMiddleware` e criar `SentryScopeEnrichmentMiddleware` para adicionar tags de contexto (rota, método, request_id, tenantId, userId/role), registrando no pipeline após autenticação.

## Requisitos
- Chamar `SentrySdk.CaptureException(exception)` no handler global
- Criar middleware de escopo e registrar `app.UseMiddleware<...>()`
- Incluir tags: `http.method`, `route`, `request_id`, `tenantId`, `userId`, `role`
- Evitar PII (email opcional; seguir política de scrub)

## Subtarefas
- [x] 2.1 Alterar `GlobalExceptionHandlerMiddleware` para capturar no Sentry
- [x] 2.2 Implementar `SentryScopeEnrichmentMiddleware`
- [x] 2.3 Registrar middleware (após Auth, antes dos controllers)
- [x] 2.4 Validar tags em eventos de erro gerados localmente

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 9.0, 10.0
- Paralelizável: Não (depende do SDK configurado)

## Detalhes de Implementação
- Seguir exemplo da Tech Spec (Middleware de escopo e captura explícita)
- Referências: tasks/prd-observabilidade-sentry/techspec.md

## Critérios de Sucesso
- Eventos no Sentry exibem tags e usuário (quando autenticado)
- Handler global continua retornando resposta segura ao cliente
- Não há PII vazando em eventos

