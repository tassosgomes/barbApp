status: completed
parallelizable: true
blocked_by: []
---

<task_context>
<domain>engine/infra/observability</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>medium</complexity>
<dependencies>external_apis|http_server</dependencies>
<unblocks>"2.0","3.0","7.0","9.0","10.0"</unblocks>
</task_context>

# Tarefa 1.0: Backend - Bootstrap Sentry SDK e configuração

## Visão Geral
Adicionar e configurar o SDK do Sentry no backend (.NET 8, ASP.NET Core) via `UseSentry`, lendo DSN/ambiente/release de configuração/variáveis. Garantir defaults seguros (sem PII) e amostragem inicial conservadora.

## Requisitos
- Adicionar pacote `Sentry.AspNetCore` ao projeto API
- Configurar `builder.WebHost.UseSentry(...)` em `Program.cs`
- Ler `Dsn`, `Environment`, `Release`, `TracesSampleRate` de `appsettings`/env
- `SendDefaultPii = false` e `IsGlobalModeEnabled = true`
- Documentar variáveis esperadas (ex.: `SENTRY_DSN`, `SENTRY_RELEASE`)

## Subtarefas
- [x] 1.1 Adicionar `Sentry.AspNetCore` e restaurar dependências
- [x] 1.2 Configurar `UseSentry` em `backend/src/BarbApp.API/Program.cs`
- [x] 1.3 Adicionar seção `Sentry` em `appsettings.json` com placeholders
- [x] 1.4 Mapear leitura por `IConfiguration` (env > appsettings)
- [x] 1.5 Documentar variáveis e flags no README do backend

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 2.0, 3.0, 7.0, 9.0, 10.0
- Paralelizável: Sim (independe do frontend/CI)

## Detalhes de Implementação
- Seguir Tech Spec em tasks/prd-observabilidade-sentry/techspec.md (seção: Backend – Pacote e Host)
- Exemplo de options: `Dsn`, `Environment`, `Release`, `TracesSampleRate`, `SendDefaultPii = false`

## Critérios de Sucesso
- Aplicação sobe com Sentry inicializado sem erros (em dev com DSN dummy)
- Eventos de erro não tratados são enviados quando ocorrerem
- Nenhum dado PII padrão é enviado (configuração segura por default)

- [x] 1.0 Backend - Bootstrap Sentry SDK e configuração ✅ CONCLUÍDA
  - [x] 1.1 Implementação completada
  - [x] 1.2 Definição da tarefa, PRD e tech spec validados
  - [x] 1.3 Análise de regras e conformidade verificadas
  - [x] 1.4 Revisão de código completada
  - [x] 1.5 Pronto para deploy
