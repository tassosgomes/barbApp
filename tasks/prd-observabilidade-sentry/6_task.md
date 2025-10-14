---
status: pending
parallelizable: true
blocked_by: []
---

<task_context>
<domain>infra/ci_cd</domain>
<type>documentation|configuration</type>
<scope>configuration</scope>
<complexity>low</complexity>
<dependencies>temporal</dependencies>
<unblocks>"1.0","4.0","5.0","7.0","8.0","10.0"</unblocks>
</task_context>

# Tarefa 6.0: Releases e Ambientes - Convenções e variáveis de CI/CD

## Visão Geral
Definir convenção de `release` e nomes de `environment`, mais variáveis/secrets de CI/CD para backend e frontend. Alinhar com pipelines existentes, documentando como injetar valores.

## Requisitos
- Convenção de release: `barbapp-<app>-<semver>-<commit>` (ou equivalente aprovado)
- Ambientes oficiais: `development`, `staging`, `production`
- Variáveis de pipeline: `SENTRY_DSN`, `SENTRY_RELEASE`, `ASPNETCORE_ENVIRONMENT`, `VITE_SENTRY_*`, `SENTRY_AUTH_TOKEN`
- Documento de referência e owners

## Subtarefas
- [ ] 6.1 Definir convenções e registrar no repositório (docs)
- [ ] 6.2 Mapear variáveis esperadas por app e por ambiente
- [ ] 6.3 Especificar passos de injeção nos pipelines (CI)

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 1.0, 4.0, 5.0, 7.0, 8.0, 10.0
- Paralelizável: Sim (documental/alinhamento de pipeline)

## Detalhes de Implementação
- PRD: Releases, Ambientes e Alertas (requisitos 4.x)
- Tech Spec: uso de `Release` e envs em backend/frontend

## Critérios de Sucesso
- Convenções aprovadas e publicadas
- Pipelines configurados com variáveis necessárias
- Times cientes do processo de versionamento e releases

