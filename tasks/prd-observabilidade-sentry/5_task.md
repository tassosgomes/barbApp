---
status: pending
parallelizable: false
blocked_by: ["6.0"]
---

<task_context>
<domain>engine/web</domain>
<type>integration|configuration</type>
<scope>build_pipeline|performance</scope>
<complexity>medium</complexity>
<dependencies>external_apis|temporal</dependencies>
<unblocks>"9.0","10.0"</unblocks>
</task_context>

# Tarefa 5.0: Frontend - Upload de sourcemaps com @sentry/vite-plugin (CI)

## Visão Geral
Configurar o `@sentry/vite-plugin` no `vite.config.ts` para upload de sourcemaps durante o build de produção no CI, autenticando via `SENTRY_AUTH_TOKEN` e usando `VITE_SENTRY_RELEASE`.

## Requisitos
- Adicionar e configurar `@sentry/vite-plugin`
- Variáveis: `SENTRY_ORG`, `SENTRY_PROJECT`, `SENTRY_AUTH_TOKEN`, `VITE_SENTRY_RELEASE`
- Ativar upload somente em builds de produção
- Documentar passos no pipeline de CI

## Subtarefas
- [ ] 5.1 Instalar `@sentry/vite-plugin`
- [ ] 5.2 Alterar `barbapp-admin/vite.config.ts` com o plugin
- [ ] 5.3 Configurar secrets e variáveis no CI/CD
- [ ] 5.4 Build de teste e verificação de artefatos no Sentry

## Sequenciamento
- Bloqueado por: 6.0 (convenção de release/env e variáveis de pipeline)
- Desbloqueia: 9.0, 10.0
- Paralelizável: Não (depende do alinhamento de release/env)

## Detalhes de Implementação
- Ver Tech Spec (seção Frontend – Vite plugin)
- Sourcemaps de `./dist/**` com release correspondente

## Critérios de Sucesso
- Sourcemaps publicados e vinculados à release no Sentry
- Stack traces desofuscados em eventos de erro
- Pipeline reproduzível e documentado

