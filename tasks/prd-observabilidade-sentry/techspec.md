# Template de Especificação Técnica

## Resumo Executivo

Implementaremos Sentry no backend (.NET 8, ASP.NET Core + Serilog) e no frontend (React + Vite + TypeScript) para rastreamento de erros, contexto por tenant/usuário e performance básica com amostragem controlada. No backend, adicionaremos o SDK `Sentry.AspNetCore`, integraremos com o pipeline mínimo (`UseSentry`) e garantiremos captura em nosso `GlobalExceptionHandlerMiddleware`. Enriqueceremos o escopo do Sentry com informações de tenant/usuário por requisição. No frontend, inicializaremos `@sentry/react` no bootstrap (`main.tsx`) com `dsn`, `environment` e `release`. Publicaremos sourcemaps via `@sentry/vite-plugin` em CI, usando variáveis seguras. Alertas e convenção de `release` serão padronizados, preservando privacidade via scrub e evitando PII.

## Arquitetura do Sistema

### Visão Geral dos Componentes

- Backend (BarbApp.API):
  - SDK Sentry para ASP.NET Core inicializado em host (`UseSentry`).
  - Middleware de exceção existente estendido para capturar exceções no Sentry.
  - Middleware de escopo Sentry (novo) para taguear `tenantId`, `userId`, `role`, rota, método HTTP.
  - Integração opcional com Serilog via sink do Sentry (futuro, se necessário).
- Frontend (barbapp-admin):
  - `@sentry/react` inicializado no bootstrap (`src/main.tsx`), com integrações padrão do browser.
  - `@sentry/vite-plugin` no `vite.config.ts` para upload de sourcemaps autenticado no CI.
- Fluxo de dados:
  - Eventos de erro/trace → Sentry (projeto Backend e projeto Frontend) → Alertas/notificações → triagem.

## Design de Implementação

### Interfaces Principais

Backend – abstração para teste (opcional, recomendada se quisermos testar captura sem estáticos):

```csharp
// Infra/Observability/ISentryReporter.cs
public interface ISentryReporter {
    void CaptureException(Exception ex);
    void SetTag(string key, string value);
    void SetUser(string id, string? email, string? username);
}
```

Implementação fina pode delegar a `SentrySdk`. Se evitarmos a abstração, usaremos diretamente `Sentry.SentrySdk` (SDK oficial) nas extensões/middlewares, cientes de menor testabilidade.

Frontend – não requer interface dedicada; inicialização ocorre no bootstrap com APIs do `@sentry/react`.

### Modelos de Dados

- Tags e contexto (backend):
  - `tenantId: string?`, `userId: string?`, `role: string?`, `route: string`, `http.method`, `http.status_code` (quando disponível), `request_id`.
- User (backend):
  - `Id`, `Email` (se não sensível), `Username`/`Role` (se aplicável). Evitar PII; campo email opcional e sujeito a scrub.
- Frontend: contexto padrão de rotas e breadcrumbs (navegação, XHR/fetch). Desabilitar/filtrar campos sensíveis.

### Endpoints de API

- Nenhuma mudança de contrato público. A integração de Sentry é transversal via middleware.

## Pontos de Integração

- Serviços externos: Sentry SaaS (sentry.io). Requisitos: DSN por projeto/ambiente, token para upload de sourcemaps no CI.
- Autenticação: não aplicável diretamente; usar DSN e `SENTRY_AUTH_TOKEN` apenas no build do frontend.
- Tratamento de erros: backend captura tanto pelo middleware do Sentry quanto explicitamente no `GlobalExceptionHandlerMiddleware` (garante captura mesmo com handler customizado).

## Análise de Impacto

| Componente Afetado                         | Tipo de Impacto               | Descrição & Nível de Risco                                   | Ação Requerida                  |
| ------------------------------------------ | ----------------------------- | ------------------------------------------------------------- | ------------------------------- |
| `backend/src/BarbApp.API/Program.cs`       | Configuração de Host          | Adicionar `UseSentry` e opções via `appsettings`/env. Baixo. | Atualizar bootstrapping         |
| `GlobalExceptionHandlerMiddleware`         | Tratamento de Erros           | Capturar no Sentry além de log. Baixo.                       | Injetar captura de exceções     |
| Novo `SentryScopeEnrichmentMiddleware`     | Middleware (escopo por req.)  | Enriquecer escopo com tenant/usuário/rota. Baixo.            | Criar e registrar middleware    |
| `barbapp-admin/src/main.tsx`               | Bootstrap do App              | Inicializar `@sentry/react`. Baixo.                          | Adicionar init                  |
| `barbapp-admin/vite.config.ts`             | Build                          | Incluir `@sentry/vite-plugin` p/ sourcemaps. Médio.          | Configurar plugin no CI         |
| Pipelines CI/CD                            | Build/Release                 | Exportar envs (DSN, release, auth token) e release tagging.  | Atualizar pipelines             |

Impacto de performance: mínimo, controlado por `sampleRate/tracesSampleRate`. Aumento de tráfego de saída (eventos). Sem mudanças de APIs de negócio.

## Abordagem de Testes

### Testes Unitários

- Backend:
  - Testar que `GlobalExceptionHandlerMiddleware` chama `CaptureException` (via abstração `ISentryReporter` ou wrapper) para exceções 500.
  - Testar que `SentryScopeEnrichmentMiddleware` popula tags quando claims e tenant estão presentes; não falha quando ausentes.
- Frontend:
  - Teste de smoke garantindo que a inicialização não quebra `main.tsx` em ambiente de teste (condicionar init a `import.meta.env.PROD` se necessário). 

### Testes de Integração

- Backend:
  - Rodar um request que dispara `/test/unhandled` e validar que o handler retorna 500 e o logger registra; em staging, verificar evento no Sentry (manual).
- Frontend:
  - Em homolog, forçar um erro de teste via `Sentry.captureException(new Error('test'))` e verificar recebimento no projeto frontend.

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. Backend – Dependências e configuração: adicionar pacote `Sentry.AspNetCore`, `UseSentry`, opções via `appsettings`/env.
2. Backend – Captura explícita no `GlobalExceptionHandlerMiddleware` e middleware de escopo (tenant/user/rota).
3. Frontend – Dependências e bootstrap: `@sentry/react` e init em `main.tsx`.
4. Frontend – Build: `@sentry/vite-plugin` e upload de sourcemaps no CI.
5. Configurar releases/ambientes e alertas no Sentry; smoke-tests em dev/homolog.

### Dependências Técnicas

- Acesso aos projetos Sentry (backend e frontend) e DSNs por ambiente.
- Token do Sentry para upload de sourcemaps (`SENTRY_AUTH_TOKEN`), organização (`SENTRY_ORG`) e projeto (`SENTRY_PROJECT`).
- Internet de saída a partir de runtime (para envio de eventos) e runners de CI (para upload de sourcemaps).

## Monitoramento e Observabilidade

- Métricas (backend): erros por 1.000 requisições (calculado no Sentry), taxa de 5xx por rota, top exceptions. Considerar expor contadores de requisição via logs/metrics existentes se necessário.
- Logs: manter Serilog como fonte primária; evitar PII conforme `rules/logging.md`. Opcionalmente, adicionar sink `Sentry.Serilog` no futuro.
- Dashboards: usar relatórios de Issues/Discover do Sentry; se já existir Grafana/Prometheus, fora do escopo.

## Considerações Técnicas

### Decisões Principais

- SDKs oficiais do Sentry: reduz esforço e traz integrações nativas (.NET/React).
- Captura explícita no `GlobalExceptionHandlerMiddleware` para garantir entrega, já que um handler custom pode short-circuit a pipeline.
- Enriquecimento de escopo com contexto de tenant/usuário melhora triagem sem expor PII.
- Sourcemaps: indispensáveis para diagnósticos no frontend; upload automatizado no build.

### Riscos Conhecidos

- Volume de eventos: mitigar com `sampleRate`, `tracesSampleRate` e filtros (beforeSend/beforeBreadcrumb no frontend; `BeforeSend` no backend).
- PII acidental: habilitar scrub e revisar campos proibidos; não incluir corpo de request em eventos.
- Overhead: monitorar após ativação; iniciar conservador (`tracesSampleRate` baixo em prod, p.ex. 0.05).

### Requisitos Especiais

- Performance: overhead alvo <3% CPU/memória.
- Segurança: segredos via env/secret manager; não versionar DSN privados de projeto self-host.

### Conformidade com Padrões

- `rules/logging.md`: uso de `ILogger`, evitar PII, logging estruturado.
- `rules/react.md` e `rules/tests-react.md`: não bloquear UI, testes contemplando mensagens de erro e fallback.
- Boas práticas HTTP em `rules/http.md`: retornar somente mensagens seguras ao cliente (já feito pelo middleware global).

---

Implementação detalhada (proposta de difs)

Backend (.NET 8)

1) Pacote e Host
- Adicionar pacote: `Sentry.AspNetCore`
- Program.cs (host):

```csharp
// Program.cs
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"]; // ou SENTRY_DSN
    options.Environment = builder.Configuration["Sentry:Environment"] ?? builder.Environment.EnvironmentName;
    options.Release = builder.Configuration["Sentry:Release"]; // definido no CI
    options.TracesSampleRate = GetDouble(builder.Configuration["Sentry:TracesSampleRate"], defaultValue: 0.05);
    options.IsGlobalModeEnabled = true; // captura fora do contexto de request
    options.SendDefaultPii = false; // evitar PII
    options.MaxBreadcrumbs = 100;
});

static double GetDouble(string? v, double defaultValue) => double.TryParse(v, out var d) ? d : defaultValue;
```

2) Captura explícita no handler global

```csharp
// GlobalExceptionHandlerMiddleware.cs (HandleExceptionAsync)
Sentry.SentrySdk.CaptureException(exception);
```

3) Enriquecimento de escopo por requisição (novo middleware)

```csharp
// Infra/Observability/SentryScopeEnrichmentMiddleware.cs
using Microsoft.AspNetCore.Http;
using Sentry;
using BarbApp.Domain.Interfaces;
using System.Security.Claims;

public class SentryScopeEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    public SentryScopeEnrichmentMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx, ITenantContext tenant)
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("http.method", ctx.Request.Method);
            scope.SetTag("route", ctx.Request.Path);
            scope.SetTag("request_id", ctx.TraceIdentifier);
            if (tenant?.BarbeariaId != null) scope.SetTag("tenantId", tenant.BarbeariaId.ToString()!);
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = ctx.User.FindFirstValue(ClaimTypes.Role);
            if (!string.IsNullOrEmpty(userId)) scope.User = new User { Id = userId, Other = new() { ["role"] = role ?? string.Empty } };
        });
        await _next(ctx);
    }
}

// Registro no pipeline (Program.cs):
app.UseMiddleware<SentryScopeEnrichmentMiddleware>(); // após Auth, antes dos controllers
```

4) Configuração (`appsettings.json`)

```json
"Sentry": {
  "Dsn": "${SENTRY_DSN}",
  "Environment": "${ASPNETCORE_ENVIRONMENT}",
  "Release": "${SENTRY_RELEASE}",
  "TracesSampleRate": 0.05
}
```

Frontend (React + Vite)

1) Dependências
- `@sentry/react` e `@sentry/vite-plugin`

2) Bootstrap (`src/main.tsx`)

```ts
// src/main.tsx
import * as Sentry from '@sentry/react';

Sentry.init({
  dsn: import.meta.env.VITE_SENTRY_DSN,
  environment: import.meta.env.VITE_SENTRY_ENVIRONMENT || import.meta.env.MODE,
  release: import.meta.env.VITE_SENTRY_RELEASE,
  integrations: [Sentry.browserTracingIntegration()],
  tracesSampleRate: Number(import.meta.env.VITE_SENTRY_TRACES_SAMPLE_RATE ?? 0.05),
  beforeSend(event) {
    // Remover dados sensíveis
    if (event.request?.headers) {
      delete (event.request.headers as any)['authorization'];
    }
    return event;
  },
});
```

3) Vite plugin (upload de sourcemaps no CI)

```ts
// vite.config.ts
import { sentryVitePlugin } from '@sentry/vite-plugin';

export default defineConfig({
  plugins: [
    react(),
    sentryVitePlugin({
      org: process.env.SENTRY_ORG,
      project: process.env.SENTRY_PROJECT,
      authToken: process.env.SENTRY_AUTH_TOKEN,
      release: process.env.VITE_SENTRY_RELEASE,
      sourcemaps: { assets: './dist/**' },
      disable: process.env.NODE_ENV !== 'production',
    }),
  ],
});
```

4) Variáveis de ambiente (frontend)
- Runtime: `VITE_SENTRY_DSN`, `VITE_SENTRY_ENVIRONMENT`, `VITE_SENTRY_RELEASE`, `VITE_SENTRY_TRACES_SAMPLE_RATE`.
- Build (CI): `SENTRY_ORG`, `SENTRY_PROJECT`, `SENTRY_AUTH_TOKEN`, além de `VITE_SENTRY_RELEASE`.

Releases e Ambientes (CI/CD)

- Convenção de release: `barbapp-<app>-<semver>-<commit>`.
- Backend: exportar `SENTRY_RELEASE` na execução/deploy e `Sentry:Release` via env/args.
- Frontend: exportar `VITE_SENTRY_RELEASE` antes de `vite build` e plugin Sentry.
- Ambientes: `development`, `staging`, `production`.

Filtros e Scrub

- Backend: `options.SendDefaultPii = false`; usar `BeforeSend` se necessário para remover headers/cookies.
- Frontend: `beforeSend` para remover headers sensíveis, campos de formulário, etc.

Alertas (no Sentry)

- Regras por projeto: erro novo com >N ocorrências em 5 min; spike de taxa; afetando >X usuários únicos.
- Canais: e-mail e/ou Slack (definir no provisionamento da conta Sentry).

## Questões Abertas e Esclarecimentos Técnicos

- Projetos no Sentry: um por app (backend/ frontend) ou unificado? Recomendado um por app.
- Convenção final de release e fonte de `semver` (package.json/AssemblyInfo) e `commit` (CI `GIT_SHA`).
- Sample rates por ambiente (ex.: dev/homolog 1.0 para erros; prod 1.0 erros e 0.05 traces).
- Lista de campos a serem sempre scrubados (headers, query params, paths com PII).
- Preferência por abstração `ISentryReporter` no backend para testabilidade vs uso direto de `SentrySdk`.

## Caminho e Operação de Escrita

- Especificação escrita em: `tasks/prd-observabilidade-sentry/techspec.md`

